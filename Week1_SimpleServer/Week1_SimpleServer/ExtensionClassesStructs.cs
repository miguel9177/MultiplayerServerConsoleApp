using System.Numerics;
using System.Runtime.Serialization;


//THIS SCRIPT HANDLES ALL EXTENSIONS TO THE CODE

namespace Extensions.Vector
{
    public class Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x_, float y_, float z_)
        {
            x = x_;
            y = y_;
            z = z_;
        }

        public Vector3()
        {
            x = 0;
            y = 0;
            z = 0;
        }
    }

    public class Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x_, float y_, float z_, float w_)
        {
            x = x_;
            y = y_;
            z = z_;
            w = w_;
        }

        public Quaternion()
        {
            x = 0;
            y = 0;
            z = 0;
            w = 0;
        }
    }
}
