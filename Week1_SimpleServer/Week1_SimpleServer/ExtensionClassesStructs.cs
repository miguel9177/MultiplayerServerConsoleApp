using System.Numerics;
using System.Runtime.Serialization;


//THIS SCRIPT HANDLES ALL EXTENSIONS TO THE CODE
namespace Extensions.Vector
{
    //vector 3 class with all the necessary math 
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

        //this gets the forward vector
        public static Vector3 forward
        {
            get 
            {
                return new Vector3(1,0,0);
            }
        }

        //this normalizes a vector, and returns the normalized one 
        public Vector3 normalized
        {
            get
            {
                float magnitude = Magnitude(this);
                return new Vector3(x / magnitude, y / magnitude, z / magnitude);
            }
        }

        #region Operation overloading
        public static Vector3 operator +(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.x + vector2.x, vector1.y + vector2.y, vector1.z + vector2.z);
        }

        public static Vector3 operator -(Vector3 vector1, Vector3 vector2)
        {
            return new Vector3(vector1.x - vector2.x, vector1.y - vector2.y, vector1.z - vector2.z);
        }

        public static Vector3 operator *(Vector3 vector1, float amountToMultiply)
        {
            return new Vector3(vector1.x * amountToMultiply, vector1.y * amountToMultiply, vector1.z * amountToMultiply);
        }

        public static Vector3 operator /(Vector3 vector1, float amountToDivide)
        {
            return new Vector3(vector1.x / amountToDivide, vector1.y / amountToDivide, vector1.z / amountToDivide);
        }
        #endregion

        //this returns the magnitude of a vector
        public static float Magnitude(Vector3 vector1)
        {
            return (float)Math.Sqrt(vector1.x * vector1.x + vector1.y * vector1.y + vector1.z * vector1.z);
        }

        //this returns the distance between on vector and another
        public static float Distance(Vector3 vector1, Vector3 vector2)
        {
            float x = vector1.x - vector2.x;
            float y = vector1.y - vector2.y;
            float z = vector1.z - vector2.z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        //dot product
        public static float Dot(Vector3 vector1, Vector3 vector2)
        {
            return vector1.x * vector2.x + vector1.y * vector2.y + vector1.z * vector2.z;
        }

        //returns the angle between 2 vectors
        public static float Angle(Vector3 vector1, Vector3 vector2)
        {
            float dot = Dot(vector1, vector2);
            float magnitude = Magnitude(vector1) * Magnitude(vector2);
            float angle = (float)Math.Acos(dot / magnitude);
            return angle * (180f / (float)Math.PI);
        }

        //trans a quaternion to a fwd vector
        public static Vector3 QuaternionToFwdVector(Quaternion q)
        {
            //WORKING THE OTHER WAY AROUND
            return new Vector3(
                1 - 2 * (q.y * q.y + q.z * q.z),
                2 * (q.x * q.y + q.w * q.z),
                2 * (q.x * q.z - q.w * q.y)
            );
        }
    }

    //this is a quaternion class, for me to be able to store the character rotation
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
