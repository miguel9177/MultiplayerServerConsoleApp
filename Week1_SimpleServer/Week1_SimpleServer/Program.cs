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
        static IPEndPoint[] sender = new IPEndPoint[30];
        //TO-DO
        static EndPoint[] allClients = new EndPoint[30];

        //this stores the ip adress
        static string serverIpAdress = "192.168.0.38";

        static void Main(string[] args)
        {
            initializeServer();

            Thread thr1 = new Thread(SendData);
            Thread thr2 = new Thread(KeyCheker);
            Thread thr3 = new Thread(ReceiveData);

            thr1.Start();
            thr2.Start();
            thr3.Start();
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
                for (int i = 0; i < allClients.Length; i++)
                {
                    //if this connection is not null, we send the data to him
                    if (allClients[i] != null)
                    {
                        //we make sure the data variable is always the same size
                        data = new byte[1024];
                        //we transform the player info into bytes, since we need to convert everything into bytes in order to send it
                        data = Encoding.ASCII.GetBytes("client index: " + i);
                        //send the bytes to the current conection. First parameter is the data, 2nd is packet size, 3rd is any flags we want, and 4th is destination client.
                        newsock.SendTo(data, data.Length, SocketFlags.None, allClients[i]);
                    }

                }

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
                
                for(int i = 0; i < allClients.Length; i++)
                {
                    if (allClients[i] == null)
                    {
                        allClients[i] = newRemote;
                        return;
                    }
                }
            }
            //received a Message that is not the first one
            else
            {
                
                PlayerInfoClass playerInfo = ObjectsSerializer.Deserialize<PlayerInfoClass>(data); //data.Deserialize<PlayerInfoClass>();
                if (playerInfo != null)
                {
                    Console.WriteLine("Received player info-> position: " + playerInfo.position + " rotation: " + playerInfo.rotation);
                    // Do something with receivedClass
                    //Console.WriteLine("Received this message from the ip: " + newRemote.ToString() + " and the message is " + text);
                }
                else
                {
                    // The received data is not of the expected type
                    Console.WriteLine("The data received was not a playerInfoClass");
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

    }
}
