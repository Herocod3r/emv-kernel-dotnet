using System;
using System.Collections.Generic;
using Kernel.Utils;
using static Kernel.Tlv.BerHelpers;

namespace Kernel.Tlv
{
    public class Tlv : Dictionary<int,byte[]>
    {
        public void Decode(byte[] data)
        {
            for (int i = 0; i < data.Length;)
            {
               
                var (tag, tagLength) = DecodeTag(Helpers.ArraySlice(data,i,data.Length));
                i += tagLength;
                if(tag == 0) continue;
                var (length, lengthLength) = DecodeLength(Helpers.ArraySlice(data, i, data.Length));
                i += lengthLength;
                var value = Helpers.ArraySlice(data, i, i + (int) length);
                i += (int)length;
                this[tag] = value;
            }
        }

        public byte[] Encode()
        {
            var data = new List<byte>();
            foreach (var item in this)
            {
                data.AddRange(EncodeTag(item.Key));
                data.AddRange(EncodeLength((ulong) item.Value.Length));
                data.AddRange(item.Value);
            }

            return data.ToArray();
        }

        public void CopyFrom(Tlv other)
        {
            foreach (var item in other)
            {
                this[item.Key] = item.Value;
            }
        }
       
    }
}