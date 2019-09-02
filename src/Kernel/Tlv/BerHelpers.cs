using System;
using System.Collections.Generic;
using static Kernel.Utils.Helpers;

namespace Kernel.Tlv
{
    public static class BerHelpers
    {
        public static (ulong, int) DecodeLength(byte[] toParse)
        {
            if(toParse[0] == 0x80) throw new ArgumentException("Indefinate length not supported");

            if ((toParse[0] & 0x80) == 0) return ((ulong) toParse[0], 1);

            var numOctets = (int) toParse[0] & 0x7F;
            if(toParse.Length < 1+numOctets) throw new ArgumentException("invalid length");
            var val = DecodeUint(ArraySlice(toParse,1, numOctets + 1));
            return (val,1 + numOctets);

        }


        public static byte[] EncodeTag(int tag)
        {
            if(((tag >> 8)&0x1F) == 0) return new byte[]{(byte)tag};
            return new byte[]{(byte)(tag >> 8),(byte)(tag & 0xFF)};
        }
        
        
        
        public static byte[] EncodeLength(ulong length)
        {
            if(length <= 0x7F) return new byte[]{(byte)length};
            var r = EncodeUint(length);
            var numOctets = r.Length;
            var result = new byte[1+numOctets];
            result[0] = (byte) (0x80 | (byte)numOctets);
            for (int i = 0; i < r.Length; i++)
            {
                result[i + 1] = r[i];
            }

            return result;
        }

        public static byte[] EncodeUint(ulong toEncode)
        {
            var l = 1;
            for (var i = toEncode; i > 255; i >>= 8)
            {
                l++;
            }

            var result = new byte[l];
            for (var i = 0; i < l; i++)
            {
                result[i] = (byte) (toEncode >>  (8 * (l - i - 1)));
            }

            return result;
        }

        public static ulong DecodeUint(byte[] toParse)
        {
            if (toParse.Length > 8) throw new ArgumentException("This integer would overflow");
            ulong val = 0;
            foreach (var b in toParse)
            {
                val = val << 8 | b;
            }

            return val;
        }

       
        public static (int, int) DecodeTag(byte[] toParse)
        {
            if ((toParse[0] & 0x1F) != 0x1F) return ((int) toParse[0], 1);
            return (((int) toParse[0] << 8) | (int) toParse[1], 2);
        }

    }
}