using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace SpeedRunCommon
{
    public static class ObjectHelper
    {
        public static long GetMemorySize(this object obj)
        {
            long result = 0;
            using (Stream s = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(s, obj);
                result = s.Length;
            }

            return result;
        }
    }
}
