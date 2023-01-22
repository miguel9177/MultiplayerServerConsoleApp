using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;


namespace Server
{
    internal class Program
    {
        static Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); //make a socket using UDP. The parameters passed are enums used by the constructor of Socket to configure the socket.
        static IPEndPoint[] sender = new IPEndPoint[30];
        static EndPoint[] Remote = new EndPoint[30];
        static string serverIpAdress = "192.168.0.38";
        static string playerInfo = "";
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


        static void initializeServer()
        {
            //task 1

            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(serverIpAdress), 9050); //our server IP. This is set to local (127.0.0.1) on socket 9050. If 9050 is firewalled, you might want to try another!


            newsock.Bind(ipep); //bind the socket to our given IP
            Console.WriteLine("Socket open..."); //if we made it this far without any networking errors, it’s a good start!
        }

        static private void SendData()
        {
            byte[] data = new byte[1024];



            while (true)
            {
                for (int i = 1; i < Remote.Length; i++)
                {
                    if (Remote[i] != null)
                    {
                        //TimeSpan currentTime = DateTime.Now.TimeOfDay;
                        data = new byte[1024];
                        data = Encoding.ASCII.GetBytes(playerInfo); //remember we need to convert anything to bytes to send it
                        newsock.SendTo(data, data.Length, SocketFlags.None, Remote[i]);//send the bytes for the ‘hi’ string to the Remote that just connected. First parameter is the data, 2nd is packet size, 3rd is any flags we want, and 4th is destination client.

                    }

                }

            }
            //newsock.SendTo(data, data.Length, SocketFlags.None, newRemote); //send the bytes for the ‘hi’ string to the Remote that just connected. First parameter is the data, 2nd is packet size, 3rd is any flags we want, and 4th is destination client.

        }

        static private void ReceiveData()
        {


            byte[] data = new byte[1024]; // the (expected) packet size. Powers of 2 are good. Typically for a game we want small, optimised packets travelling fast. The 1024 bytes chosen here is arbitrary – you should adjust it.
            int recv;

            //task 2
            int pos = 0;


            //ConsoleKey keyCheck;
            while (true)
            {

                sender[pos] = new IPEndPoint(IPAddress.Any, 0);
                if (Remote[pos] == null)
                {
                    Remote[pos] = (EndPoint)(sender[pos]);
                }


                //IPEndPoint newSender = new IPEndPoint(IPAddress.Any, 0);
                //EndPoint newRemote = Remote[pos];

                //if (Remote[pos] != null) { 
                //    newRemote = Remote[pos];
                //}

                EndPoint newRemote = Remote[pos];
                data = new byte[1024];
                recv = newsock.ReceiveFrom(data, ref newRemote); //recv is now a byte array containing whatever just arrived from the client
                //EndPoint newRemote = Remote[pos];
                Console.WriteLine("Message received from " + newRemote.ToString()); //this will show the client’s unique id
                //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv)); //and this will show the data
                string text = Encoding.ASCII.GetString(data, 0, recv); //and this will show the data
                playerInfo = Encoding.ASCII.GetString(data, 0, recv);
                Console.WriteLine(playerInfo);
                if (text == "FirstEntrance")
                {
                    string hi = "Yep, you just connected!";
                    data = Encoding.ASCII.GetBytes(hi); //remember we need to convert anything to bytes to send it
                    newsock.SendTo(data, data.Length, SocketFlags.None, newRemote); //send the bytes for the ‘hi’ string to the Remote that just connected. First parameter is the data, 2nd is packet size, 3rd is any flags we want, and 4th is destination client.
                    pos = pos + 1;
                    Remote[pos] = newRemote;

                }
                else
                {
                    //SendData();
                    //string hi = "Ganda Maluco";
                    //data = Encoding.ASCII.GetBytes(hi); //remember we need to convert anything to bytes to send it
                    //newsock.SendTo(data, data.Length, SocketFlags.None, newRemote); //send the bytes for the ‘hi’ string to the Remote that just connected. First parameter is the data, 2nd is packet size, 3rd is any flags we want, and 4th is destination client.

                }



                //bool newConnect = false;
                //for (int i = 0; i < newRemoteList.Length; i++)
                //{
                //    if (newRemoteList[i] == Remote[i])
                //    {
                //        newConnect = true;
                //        break;

                //    }

                //}
                //if(!newConnect)
                //    pos = pos + 1;



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
