using System;
using System.Collections.Generic;
using Kernel.Tlv;

namespace Kernel.Emv
{
    public class ApplicationFileList : List<ApplicationFile>,ITlvDecoder
    {
        public void Decode(byte[] bytes)
        {
            if(bytes.Length%4 != 0)throw new ArgumentException("Length of bytes must be in multiples of 4");
            for (int i = 0; i < bytes.Length; i+=4)
            {
                Add(new ApplicationFile
                {
                    Sfi = bytes[i]>>3,
                    Start = bytes[i+1],
                    End = bytes[i+2],
                    SdaCount = bytes[i+3]
                });
            }
        }
    }
}