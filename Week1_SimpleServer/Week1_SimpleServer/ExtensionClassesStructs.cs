using System.Numerics;
using System.Runtime.Serialization;


//THIS SCRIPT HANDLES ALL EXTENSIONS TO THE CODE

namespace Extensions.Vector
{
    //this class creates a Vector3 that is serializable, since its mandatory to be able to send serializable data through the network, and the normal vector 3 isnt serializable
    [DataContract]
    public class Vector3Serializable
    {
        [DataMember]
        public float x;
        public float y;
        public float z;

        public Vector3Serializable(float x_, float y_, float z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }

        public Vector3Serializable() { }


    }
}
