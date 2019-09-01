using System;
using System.Collections.Generic;
using System.Linq;

namespace Kernel.Utils
{
    public static class Helpers
    {
       
        public static T[] ArraySlice<T>(T[] mainArray,int startIndex,int endIndex)
        {
            var size = endIndex - startIndex;
            var arr = new T[size];
            Array.Copy(mainArray,startIndex,arr,0,size);
            return arr;
        }
        
        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static byte[] HexStringToByteArray(string hex)=> Enumerable.Range(0, hex.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
            .ToArray();

       
    }
}