using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NHMConfigGen
{
    public static class Model
    {
        public static byte[] StringToByteArr(string s)
        {
            byte[] arr = new byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < (s.Length % 16); i++)
            {
                arr[i] = (byte)s[i];
            }
            return arr;
        }

        public static string ByteArrToString(byte[] bytes)
        {
            string hex = BitConverter.ToString(bytes);
            return hex.Replace("-", "");
        }

    }
}
