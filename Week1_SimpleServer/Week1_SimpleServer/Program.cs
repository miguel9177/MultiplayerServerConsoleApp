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

        //this stores the ip adress
        static string serverIpAdress = "127.0.0.1";

        //we start with id 12
        static int lastAssignedGlobalID = 12;

        //this stores the gamestate, it stores the client id, and then its playerInfoClass
        static Dictionary<EndPoint, PlayerInfoClass> gameState = new Dictionary<EndPoint, PlayerInfoClass>(); 

        static void Main(string[] args)
        {
            //this initializes the server port
            initializeServer();

            //creates a send data thread, that sends all data
            Thread thr1 = new Thread(SendData);
            //creates a keychecker thread, that checks if the userclciked a key
            Thread thr2 = new Thread(KeyCheker);
            //creates a thread that receives the data
            Thread thr3 = new Thread(ReceiveData);
            //creates a thread that checks all connection
            Thread thr4 = new Thread(CheckConnections);
            //initializes all threads
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
            Console.WriteLine("Socket open...");
        }

        //this runs in a thread and sends the data to the player
        static private void SendData()
        {
            //infinite loop to always send data to the players
            while (true)
            {
                //loop through every connection
                foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
                {
                    //if theres a value
                    if (kvp1.Value != null)
                    {
                        //if the player is banned we skip him, so that he stops receiving messages from the server
                        if (kvp1.Value.banned)
                            continue;

                        //this loops through every connection, so that we can send every player info to every client
                        foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp2 in gameState.ToList())
                        {
                            //if the value is not null
                            if (kvp2.Value != null)
                            {
                                //we convert the player info class into byte data
                                byte[] dataToSend = ConvertPlayerInfoClassToByte(kvp2.Value);
                                //we send the data to the client
                                newsock.SendTo(dataToSend, dataToSend.Length, SocketFlags.None, kvp1.Key);
                            }
                        }
                    }
                }
                //we add a thread sleep to not flud the clients with requests
                Thread.Sleep(50);
            }
        }

        //this runs in a thread and receives the player data
        static private void ReceiveData()
        {
            //this variable will store the data received from the player
            byte[] data = new byte[1024]; 
            
            int recv;

            //this is the infinite loop to be able to always receive the data
            while (true)
            {
                //this will get the remote connection from the client
                EndPoint newRemote = new IPEndPoint(IPAddress.Any, 0);
                //we make sure the variable data is the same size
                data = new byte[1024];
                
                //we initilialize the recv variable
                recv = data.Length;

                try
                {
                    //we receive the message from any player
                    recv = newsock.ReceiveFrom(data, ref newRemote); //recv is now a byte array containing whatever just arrived from the client
                }
                //if we couldnt receive the message, we try to remove the player from the server
                catch(Exception e)
                {
                    gameState.Remove(newRemote);
                }
                
                //this will check wich type of message the server received
                ReceivedMessageFromClientManager(data, recv, newRemote);
            }
        }

        //this is called every time the server receives a message
        static void ReceivedMessageFromClientManager(byte[] data, int recv, EndPoint newRemote)
        {
            //this gets the text received
            string text = Encoding.ASCII.GetString(data, 0, recv);

            //if the new connection already exists 
            if (gameState.ContainsKey(newRemote))
            {
                //if the player that send the message is banned we leave
                if (gameState[newRemote].banned)
                    return;
            }

            //if the text received is FirstEntrance, it means this is a new connection
            if (text == "FirstEntrance")
            {
                //since this received a new connection, it will handle the new connection message, by adding a new client
                FirstEntrance(newRemote);
            }
            //is this packet a UID request
            else if (text.Contains("I need a UID for local object:"))
            {
                //this gives a new uid to the client
                GiveNewUid(text, newRemote);
            }
            //received a Message that is Object Data
            else if (text.Contains("Object data;"))
            {
                //we update the object data
                UpdateObjectData(text, data, newRemote);
            }
            //if its an gameplay event
            else if (text.Contains("GameplayEvent:"))
            {
                //we call the function that handles events
                ReceivedGameplayEventMensage(data, text, newRemote);
            }
        }

        //this will check for key inputs
        static private void KeyCheker()
        {
            //does an infinite loop
            while (true)
            {
                //if the user clicks escape, we close the server
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    Environment.Exit(0);
                    return;
                }
            }
        }

        //this checks the connections
        static private void CheckConnections()
        {
            int playerNumber = 0;
            //does an infinite loop to always check how many connections there is
            while (true)
            {
                //loop through every connection
                foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
                {
                    //if the value is not null we increase the player number
                    if (kvp1.Value != null)
                        playerNumber++;
                }
                //we write in the console how many players are connected
                Console.WriteLine("Players Connected: " + playerNumber);
                playerNumber = 0;

                //we make the thread sleep for 5 seconds
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

            lastAssignedGlobalID += 1;
            //assign the ID
            int globalId = lastAssignedGlobalID;
            //we add the new connection to the gamestate list
            gameState.Add(_newRemote, new PlayerInfoClass 
            { 
                uniqueNetworkID = lastAssignedGlobalID,
                hp = 100,
                position = new Vector3(),
                rotation = new Quaternion()
            });
        }

        //this gives a new UID (unique id identifier)
        private static void GiveNewUid(string _text, EndPoint _newRemote)
        {
            //loop through every connection
            foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
            {
                //if current gamestate has the same key has the new remote
                if (kvp1.Key.ToString() == _newRemote.ToString())
                {
                    //parse the string into an int to get the local ID
                    int localObjectNumber = Int32.Parse(_text.Substring(_text.IndexOf(':') + 1));
                    //this creates a message telling the client their global id
                    string returnVal = ("Assigned UID:" + localObjectNumber + ";" + lastAssignedGlobalID++);
                    //we send the new uid to the clients
                    newsock.SendTo(Encoding.ASCII.GetBytes(returnVal), Encoding.ASCII.GetBytes(returnVal).Length, SocketFlags.None, _newRemote);
                }
            }
          
        }
        
        //this receives a update of an object, and stores it on the gamestate correct id
        private static void UpdateObjectData(string _text, byte[] _data, EndPoint newRemote_)
        {            
            //if true, we're already tracking the object
            if (gameState.ContainsKey(newRemote_))
            {
                //store the player info, converting the received data to bytes
                gameState[newRemote_] = ConvertByteToPlayerInfoClass(newRemote_, _data); 
            }
            //the object is new to the game
            else
            {
                //since this is a new player, we add this item to the server
                gameState.Add(newRemote_, ConvertByteToPlayerInfoClass(newRemote_, _data));
            }
        }

        //this receives a byte information, and converts it into player data
        private static PlayerInfoClass ConvertByteToPlayerInfoClass(EndPoint _newRemote, byte[] _byte_infoOfPlayer)
        {
            //we convert the bytes to string
            string _string_InfoOfPlayer = Encoding.ASCII.GetString(_byte_infoOfPlayer);

            //we split every value by ; which is how we split the info in this project
            string[] values = _string_InfoOfPlayer.Split(';');

            //we get the unique id
            int _uniqueNetworkID = Int32.Parse(values[1]);

            //we change the cultural info, since in my pc it was causing problems since its a portuguese pc
            CultureInfo ci = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            ci.NumberFormat.CurrencyDecimalSeparator = ".";

            //we get the positions
            float posX = float.Parse(values[2], NumberStyles.Any, ci);
            float posZ = float.Parse(values[3], NumberStyles.Any, ci);
            float posY = float.Parse(values[4], NumberStyles.Any, ci);

            //we get the rotations
            float rotX = float.Parse(values[5], NumberStyles.Any, ci);
            float rotZ = float.Parse(values[6], NumberStyles.Any, ci);
            float rotY = float.Parse(values[7], NumberStyles.Any, ci);
            float rotW = float.Parse(values[8], NumberStyles.Any, ci);

            //we create a new player in fo received class
            PlayerInfoClass playerInfoReceived = new PlayerInfoClass();
            //we store the returned info from the server into the new player info class, in this line we store the position
            playerInfoReceived.position = new Vector3(posX / -100, posY / 100, posZ / 100);
            //in this line we store the rotation received
            playerInfoReceived.rotation = new Quaternion(rotX, rotY, rotZ, rotW);
            //in this line we store the unique id received
            playerInfoReceived.uniqueNetworkID = _uniqueNetworkID;
            //in this line we store the hp received
            playerInfoReceived.hp = gameState[_newRemote].hp;
            //in this line we store the player info received
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
                                _infoOfPlayer.rotation.w + ";" +
                                _infoOfPlayer.hp;
                                ;

            //this converts the string to bytes
            return Encoding.ASCII.GetBytes(returnVal);
        }

        #endregion

        #region Event Message helper functions

        //this is called when we receive an gameplay event message
        private static void ReceivedGameplayEventMensage(byte[] data_, string text_, EndPoint newRemote_)
        {
            //if the event was a player shot another player
            if (text_.Contains("Player shot another player:"))
            {
                //we call the function that handles the shooting
                ReceivedPlayerShotAnotherPlayerEvent(text_, newRemote_);
            }
        }

        //this is called when we receive an shot another player event
        private static void ReceivedPlayerShotAnotherPlayerEvent(string text_, EndPoint newRemote_)
        {
            //we dividde the received message
            string[] values = text_.Split(';');

            //this stores the id of the shooting player
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
                //this is called if the player is using a not valide weapon, it will
                DisconnectPlayer("Shooting Player weapon is a not valide weapon", _uniqueNetworkIdOfShootingPlayer);
                return;
            }

            //this does the shooting calculations, and checks if the shot was valid, if it was not we kick the player from the session, since he either is cheating or hes internet is so slow that made one packet of diference be a giant diference
            if (CheckIfShootingAnotherPlayerIsNotValid(_uniqueNetworkIdOfShootingPlayer, _uniqueNetworkIdOfPlayerThatTookDamage, weaponClassOfShootingPlayer))
                DisconnectPlayer("Ban: player is shooting in an invalid way", _uniqueNetworkIdOfShootingPlayer); //this disconnects the player
            //if the shooting was valid
            else
                GiveDamageToPlayer(_uniqueNetworkIdOfShootingPlayer, _uniqueNetworkIdOfPlayerThatTookDamage, weaponClassOfShootingPlayer);//we give damage to the player
        }

        //this gives damage to the player
        private static void GiveDamageToPlayer(int _idOfShootingPlayer, int _idOfPlayerThatTookDamage, WeaponParentClass _weaponThatShot)
        {
            //we loop through all players
            foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
            {
                //if the current gamestate is the player that took the damage, decrease the hp (since all hp is calculated on the server)
                if (kvp1.Value.uniqueNetworkID == _idOfPlayerThatTookDamage)
                    gameState[kvp1.Key].hp -= _weaponThatShot.damage;
            }
        }

        #endregion

        #region Functionality helper functions

        //this checks if the shooting from one player to another is valid
        private static bool CheckIfShootingAnotherPlayerIsNotValid(int _idOfShootingPlayer, int _idOfPlayerThatTookDamage, WeaponParentClass _weaponThatShot)
        {
            //this is the angle tolerance, we need the angle tolerance since the player has a width and heidth and the angle is calculated on the player origin, we give it a big tolerance so that we dont kick anyone in an unfair way
            float angleTolerance = 40;

            //this will store the class of the player that shot
            PlayerInfoClass playerThatShot = null;
            //this will store the class of the player that got shot
            PlayerInfoClass playerThatReceivedTheShot = null;

            //we loop through all players and store who shot and who got shot
            foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
            {
                if (kvp1.Value.uniqueNetworkID == _idOfShootingPlayer)
                    playerThatShot = gameState[kvp1.Key];

                if (kvp1.Value.uniqueNetworkID == _idOfPlayerThatTookDamage)
                    playerThatReceivedTheShot = gameState[kvp1.Key];
            }

            //if the value is null, we return that the shooting was invalid
            if (playerThatShot == null)
                return true;

            //if the value is null, we return that the shooting was invalid
            if (playerThatReceivedTheShot == null)
                return true;

            //this stores the distance from the players
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
            return angle >= angleTolerance;                 
        }

        //this disconnects the player from the server
        private static void DisconnectPlayer(string disconnectReason, int idOfPlayer)
        {
            //if the disconnect reason was that the player was shooting in an invalid way, we kick him
            if(disconnectReason.Contains("Ban: player is shooting in an invalid way"))
            {
                //loop through every player
                foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
                {
                    //if the player is the correct one, we put the bool banned to true
                    if (kvp1.Value.uniqueNetworkID == idOfPlayer)
                        gameState[kvp1.Key].banned = true;
                }
            }
            //if the disconnect reason was that he was using an invalid weapon
            else if(disconnectReason.Contains("Shooting Player weapon is a not valide weapon"))
            {
                //we loop through every player
                foreach (KeyValuePair<EndPoint, PlayerInfoClass> kvp1 in gameState.ToList())
                {
                    //if the player is the correct one we put the is banned to true
                    if (kvp1.Value.uniqueNetworkID == idOfPlayer)
                        gameState[kvp1.Key].banned = true;
                }
            }
        }

        #endregion
    }
}
