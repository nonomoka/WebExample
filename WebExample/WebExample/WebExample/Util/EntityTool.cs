using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;

namespace WebExample.Util
{
    public class EntityTool
    {
        private static readonly BinaryFormatter Bf;

        static EntityTool()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
            Bf = new BinaryFormatter();
        }

        public static T Deserialize<T>(byte[] data)
        {
            var ms = new MemoryStream(data);
            return (T)Bf.Deserialize(ms);
        }

        public static byte[] Serialize(object obj)
        {
            var ms = new MemoryStream();
            Bf.Serialize(ms, obj);
            return ms.ToArray();
        }
    }
}