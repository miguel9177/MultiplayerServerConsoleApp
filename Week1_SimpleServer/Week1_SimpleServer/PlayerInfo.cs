using Extensions.Vector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.Serialization;



//this has all the player info to send to the network at every possible way, since this info is vital
[DataContract]
public class PlayerInfoClass
{
    [DataMember]
    public Vector3Serializable position;
    [DataMember]
    public Vector3Serializable rotation;

    public PlayerInfoClass()
    {
        position = new Vector3Serializable(0, 0, 0);
        rotation = new Vector3Serializable(0, 0, 0);
    }
}