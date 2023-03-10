using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Serializer;


namespace Server
{
    internal class Program
    {
        //make a socket using UDP. The parameters passed are enums used by the constructor of Socket to configure the socket.
        static Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        //TO-DO
        static List<IPEndPoint> sender = new List<IPEndPoint>();
        //TO-DO
        static List<EndPoint> allClients = new List<EndPoint>();

        //this stores the ip adress
        static string serverIpAdress = "10.1.129.150";

        static int lastAssignedGlobalID = 12; //I arbitrarily start at 12 so it’s easy to see if it’s working 😊z

        static Dictionary<int, byte[]> gameState = new Dictionary<int, byte[]>(); //initialise this at the start of the program

        static void Main(string[] args)
        {
            initializeServer();

            Thread thr1 = new Thread(SendData);
            Thread thr2 = new Thread(KeyCheker);
            Thread thr3 = new Thread(ReceiveData);
            Thread thr4 = new Thread(CheckConnections);
            thr1.Start();
            thr2.Start();
            thr3.Start();
            thr4.Start();
        }


        //this initializes the server, opening the socket
        static void initializeServer()
        {
            //create the endpoint using the specific ip adress and utilizing the port 9050
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIpAdress), 9050); 

            newsock.Bind(ipep); //bind the socket to our given IP
            Console.WriteLine("Socket open..."); //if we made it this far without any networking errors, it’s a good start!

           
        }

        //this runs in a thread and sends the data to the player
        static private void SendData()
        {
            //this variable will be used to send the data to the player
            byte[] data = new byte[1024];

            //infinite loop to always send data to the players
            while (true)
            {
                //loop through every connection (in this case we loop through 30 positions)
                for (int i = 0; i < allClients.Count; i++)
                {
                    //if this connection is not null, we send the data to him
                    if (allClients[i] != null)
                    {
                        foreach (KeyValuePair<int, byte[]> kvp in gameState.ToList())
                        {
                            newsock.SendTo(kvp.Value, kvp.Value.Length, SocketFlags.None, allClients[i]);
                        }
                    }

                }
                Thread.Sleep(50);
            }

        }

        //this runs in a thread and receives the player data
        static private void ReceiveData()
        {
            //this variable will store the data received from the player
            byte[] data = new byte[1024]; 
            
            int recv;
            
            //this variable will store the current player index we are receiving the data from
            int pos = 0;

            //this is the infinite loop to be able to always receive the data
            while (true)
            {
                EndPoint newRemote = new IPEndPoint(IPAddress.Any, 0);
                //we make sure the variable data is the same size
                data = new byte[1024];
                //we receive the message from any player
                recv = newsock.ReceiveFrom(data, ref newRemote); //recv is now a byte array containing whatever just arrived from the client
                
                //this will check wich type of message the server received
                ReceivedMessageFromClientManager(data, recv, newRemote);

                
            }
        }

        //this is called every time the server receives a message
        static void ReceivedMessageFromClientManager(byte[] data, int recv, EndPoint newRemote)
        {
            //this gets the text received
            string text = Encoding.ASCII.GetString(data, 0, recv);

            //if the text received is FirstEntrance, it means this is a new connection
            if (text == "FirstEntrance")
            {
                //we store a message to send to the client
                string hi = "Yep, you just connected!";
                Console.WriteLine("New connection with the ip " + newRemote.ToString());
                //remember we need to convert anything to bytes to send it
                data = Encoding.ASCII.GetBytes(hi);
                //we send the information to the client, so that the client knows that he just connected
                newsock.SendTo(data, data.Length, SocketFlags.None, newRemote);

                sender.Add((IPEndPoint)newRemote);
                //TODO CHECK IF CONNECTION ALREADY EXISTS
                allClients.Add(newRemote);
            }
            //is this packet a UID request?
            else if (text.Contains("I need a UID for local object:"))
            {
                Console.WriteLine(text.Substring(text.IndexOf(':')));

                //parse the string into an into to get the local ID
                int localObjectNumber = Int32.Parse(text.Substring(text.IndexOf(':') + 1));
                //assign the ID
                string returnVal = ("Assigned UID:" + localObjectNumber + ";" + lastAssignedGlobalID++);
                Console.WriteLine(returnVal);
                newsock.SendTo(Encoding.ASCII.GetBytes(returnVal), Encoding.ASCII.GetBytes(returnVal).Length, SocketFlags.None, newRemote);
            }
            //received a Message that is not the first one
            else if (text.Contains("Object data;"))//TODO, CHANGE IDENTIFIER
            {
                //get the global id from the packet
                Console.WriteLine(text);
                
                string globalId = text.Split(";")[1];
                int intId = Int32.Parse(globalId);
                if (gameState.ContainsKey(intId))
                { 
                    //if true, we're already tracking the object
                    gameState[intId] = data; //data being the original bytes of the packet
                }
                else //the object is new to the game
                {
                    gameState.Add(intId, data);
                }

            }

        }

        static private void KeyCheker()
        {

            while (true)
            {
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                    return;
                }
            }
        }

        static private void CheckConnections()
        {
            int playerNumber = 0;
            while (true)
            {
                for (int i = 0; i < allClients.Count; i++)
                {
                    if (allClients[i] != null)
                        playerNumber++;
                }
                Console.WriteLine("Players Connected: " + playerNumber);
                playerNumber = 0;

                Thread.Sleep(5000);
            }

        }
       
        
    }
}
