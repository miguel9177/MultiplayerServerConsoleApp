using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Runtime.Serialization;

//THIS CODE WAS TAKEN FROM:https://bitbucket.org/stupro_hskl_betreuung_kessler/learnit_merged_ss16/raw/e5244ebb38c8fe70759e632ea4224e48f5ca5833/Unity/LearnIT_Merged/Assets/Scripts/Util/ObjectSerializationExtension.cs
//I USE THIS CODE TO BE ABLE TO SERIALIZE AN OBJECT INTO A BYTE ARRAY

namespace Serializer
{
    public static class ObjectsSerializer
    {
        //Extension class to provide serialize / deserialize methods to object.
        //src: http://stackoverflow.com/questions/1446547/how-to-convert-an-object-to-a-byte-array-in-c-sharp
        //NOTE: You need add [Serializable] attribute in your class to enable serialization

        public static byte[] Serialize<T>(T obj)
        {
            using (var memoryStream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(memoryStream, obj);
                return memoryStream.ToArray();
            }
        }

        public static T Deserialize<T>(byte[] byteArray)
        {
            using (var memoryStream = new MemoryStream(byteArray))
            {
                var serializer = new DataContractSerializer(typeof(T));
                return (T)serializer.ReadObject(memoryStream);
            }
        }
    }
}


