using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Serializer;
using System.Globalization;
using Extensions.Vector;
using Week1_SimpleServer;

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
        static string serverIpAdress = "127.0.0.1";

        static int lastAssignedGlobalID = 12; //we start with id 12

        static Dictionary<int, PlayerInfoClass> gameState = new Dictionary<int, PlayerInfoClass>(); //this stores the gamestate, it stores the client id, and then its playerInfoClass

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
                        foreach (KeyValuePair<int, PlayerInfoClass> kvp in gameState.ToList())
                        {
                            byte[] dataToSend = ConvertPlayerInfoClassToByte(kvp.Value);
                            newsock.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, allClients[i]);
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
                //since this received a new connection, it will handle the new connection message, by adding a new client
                FirstEntrance(newRemote);
            }
            //is this packet a UID request
            else if (text.Contains("I need a UID for local object:"))
            {
                GiveNewUid(text, newRemote);
            }
            //received a Message that is Object Data
            else if (text.Contains("Object data;"))//TODO, CHANGE IDENTIFIER
            {
                UpdateObjectData(text, data);
            }
            else if (text.Contains("GameplayEvent:"))
            {
                ReceivedGameplayEventMensage(data, text, newRemote);
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

        #region Message handling Helper Functions

        //this is called a new user just connected, it sends a message saying that he just connected, and stores a new connection
        private static void FirstEntrance(EndPoint _newRemote)
        {
            //we store a message to send to the client
            string hi = "Yep, you just connected!";
            Console.WriteLine("New connection with the ip " + _newRemote.ToString());
            //remember we need to convert anything to bytes to send it
            byte[] _data = Encoding.ASCII.GetBytes(hi);
            //we send the information to the client, so that the client knows that he just connected
            newsock.SendTo(_data, _data.Length, SocketFlags.None, _newRemote);

            sender.Add((IPEndPoint)_newRemote);
            //TODO CHECK IF CONNECTION ALREADY EXISTS
            allClients.Add(_newRemote);
        }

        //this gives a new UID (unique id identifier)
        private static void GiveNewUid(string _text, EndPoint _newRemote)
        {
            Console.WriteLine(_text.Substring(_text.IndexOf(':')));

            //parse the string into an into to get the local ID
            int localObjectNumber = Int32.Parse(_text.Substring(_text.IndexOf(':') + 1));
            //assign the ID
            string returnVal = ("Assigned UID:" + localObjectNumber + ";" + lastAssignedGlobalID++);
            Console.WriteLine(returnVal);
            newsock.SendTo(Encoding.ASCII.GetBytes(returnVal), Encoding.ASCII.GetBytes(returnVal).Length, SocketFlags.None, _newRemote);
        }
        
        //this receives a update of an object, and stores it on the gamestate correct id
        private static void UpdateObjectData(string _text, byte[] _data)
        {
            //get the global id from the packet
            Console.WriteLine(_text);

            string globalId = _text.Split(";")[1];
            int intId = Int32.Parse(globalId);
            
            //if true, we're already tracking the object
            if (gameState.ContainsKey(intId))
            {
                //store the player info, converting the received data to bytes
                gameState[intId] = ConvertByteToPlayerInfoClass(_data); 
            }
            //the object is new to the game
            else
            {
                //since this is a new player, we add this item to the server
                gameState.Add(intId, ConvertByteToPlayerInfoClass(_data));
            }
        }

        //this receives a byte information, and converts it into player data
        private static PlayerInfoClass ConvertByteToPlayerInfoClass(byte[] _byte_infoOfPlayer)
        {
            string _string_InfoOfPlayer = Encoding.ASCII.GetString(_byte_infoOfPlayer);

            string[] values = _string_InfoOfPlayer.Split(';');

            int _uniqueNetworkID = Int32.Parse(values[1]);

            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            float posX = float.Parse(values[2], NumberStyles.Any, ci);
            float posZ = float.Parse(values[3], NumberStyles.Any, ci);
            float posY = float.Parse(values[4], NumberStyles.Any, ci);

            float rotX = float.Parse(values[5], NumberStyles.Any, ci);
            float rotZ = float.Parse(values[6], NumberStyles.Any, ci);
            float rotY = float.Parse(values[7], NumberStyles.Any, ci);
            float rotW = float.Parse(values[8], NumberStyles.Any, ci);

            PlayerInfoClass playerInfoReceived = new PlayerInfoClass();
            playerInfoReceived.position = new Vector3(posX / -100, posY / 100, posZ / 100);
            playerInfoReceived.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            playerInfoReceived.uniqueNetworkID = _uniqueNetworkID;

            return playerInfoReceived;
        }

        //this receives a player data, and converts it into byte information
        private static byte[] ConvertPlayerInfoClassToByte(PlayerInfoClass _infoOfPlayer)
        {
            //create a delimited string with the required data
            string returnVal = "Object data;" + _infoOfPlayer.uniqueNetworkID + ";" +
                                _infoOfPlayer.position.x * -100 + ";" +
                                _infoOfPlayer.position.z * 100 + ";" +
                                _infoOfPlayer.position.y * 100 + ";" +
                                _infoOfPlayer.rotation.x + ";" +
                                _infoOfPlayer.rotation.z + ";" +
                                _infoOfPlayer.rotation.y + ";" +
                                _infoOfPlayer.rotation.w + ";"
                                ;

            return Encoding.ASCII.GetBytes(returnVal);
        }

        #endregion

        #region Event Message helper functions

        //this is called when we receive an gameplay event message
        private static void ReceivedGameplayEventMensage(byte[] data_, string text_, EndPoint newRemote_)
        {
            if (text_.Contains("Player shot another player:"))
            {
                ReceivedPlayerShotAnotherPlayerEvent(text_, newRemote_);
            }
        }

        //this is called when we receive an shot another player event
        private static void ReceivedPlayerShotAnotherPlayerEvent(string text_, EndPoint newRemote_)
        {
            string[] values = text_.Split(';');

            //this sotres the id of the shooting player
            int _uniqueNetworkIdOfShootingPlayer = Int32.Parse(values[1]);
            //stores the id of the player that is receiving the damage
            int _uniqueNetworkIdOfPlayerThatTookDamage = Int32.Parse(values[3]);
            //stores the weapon name
            string _weaponNameOfShootingPlayer = values[5];

            //this will get the shooting player weapon, so that we can know its stats
            WeaponParentClass? weaponClassOfShootingPlayer = WeaponManager.GetWeapon(_weaponNameOfShootingPlayer);

            //if the weapon we got is invalid, we disconnect the player, since hes using an invalid weapon
            if (weaponClassOfShootingPlayer == null)
            {
                DisconnectPlayer("Shooting Player weapon is a not valide weapon", _uniqueNetworkIdOfShootingPlayer);
                return;
            }

            if (CheckIfShootingAnotherPlayerIsValid(_uniqueNetworkIdOfShootingPlayer, _uniqueNetworkIdOfPlayerThatTookDamage, weaponClassOfShootingPlayer))
                DisconnectPlayer("Ban: player is shooting in an invalid way", _uniqueNetworkIdOfShootingPlayer);
            else
                GiveDamageToPlayer();
        }

        #endregion

        #region Functionality helper functions

        //this checks if the shooting from one player to another is valid
        private static bool CheckIfShootingAnotherPlayerIsValid(int _idOfShootingPlayer, int _idOfPlayerThatTookDamage, WeaponParentClass _weaponThatShot)
        {
            float angleTolerance = 40;
            PlayerInfoClass playerThatShot = gameState[_idOfShootingPlayer];
            PlayerInfoClass playerThatReceivedTheShot = gameState[_idOfPlayerThatTookDamage];

            if (playerThatShot == null)
                return false;

            if (playerThatReceivedTheShot == null)
                return false;

            float dist = Vector3.Distance(playerThatShot.position, playerThatReceivedTheShot.position);
            //if the distance is bigger then the max distance, it means the player is in fact cheating
            if (dist > _weaponThatShot.maxRange + 5f)
                return false;

            //this converts the rotation to a forward vector
            Vector3 fwdVector = Vector3.QuaternionToFwdVector(playerThatShot.rotation);

            //this calculates the dir between the shooting and the one who got shot
            Vector3 dir = playerThatReceivedTheShot.position - playerThatShot.position;
            dir = dir.normalized;

            //gets the angle between the direction and the forward vector of the character
            double angle = Math.Acos(Vector3.Dot(fwdVector, dir)) * (180 / Math.PI);
            //we invert the angle, since it was working backwards
            angle = 180 - angle; 
            //we return if the shooting was valid or not
            return angle <= angleTolerance;                 
        }

        private static void DisconnectPlayer(string disconnectReason, int idOfPlayer)
        {

        }

        private static void GiveDamageToPlayer()
        {
            
        }

        #endregion
    }
}
