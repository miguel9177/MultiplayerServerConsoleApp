using Extensions.Vector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;



//this has all the player info to send to the network at every possible way, since this info is vital
public class PlayerInfoClass
{
    public Vector3 position;
    public Quaternion rotation;
    public int uniqueNetworkID;
    public float hp;

    public PlayerInfoClass()
    {
        position = new Vector3(0, 0, 0);
        rotation = new Quaternion(0, 0, 0, 0);
    }
}