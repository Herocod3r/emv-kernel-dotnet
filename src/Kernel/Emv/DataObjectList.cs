using System;
using System.Collections.Generic;
using Kernel.Tlv;
using Kernel.Utils;

namespace Kernel.Emv
{
    public class DataObjectList : Dictionary<int,int>,ITlvDecoder
    {
        public void Decode(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length;)
            {
                var (tag, tagLength) = BerHelpers.DecodeTag(Helpers.ArraySlice(bytes, i, bytes.Length));
                i += tagLength;
                var (length, lengthlength) = BerHelpers.DecodeLength(Helpers.ArraySlice(bytes, i, bytes.Length));
                i += lengthlength;
                this[tag] = (int) length;
            }
            
        }
    }
}