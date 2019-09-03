using System;
using System.Collections.Generic;
using Kernel.Tlv;
using Kernel.Utils;

namespace Kernel.Emv
{
    public class TagList : List<int>,ITlvDecoder
    {
        public TagList() : base()
        {
            
        }

        public void Decode(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length;)
            {
                var (tag, tagLength) = BerHelpers.DecodeTag(Helpers.ArraySlice(bytes, i, bytes.Length));
                i += tagLength;
                Add(tag);
            }
        }
    }
}