using System;

namespace Kernel.Tlv
{
    public class TlvPropertyAttribute : Attribute
    {
        public TlvPropertyAttribute(string tag)
        {
            this.Tag = Convert.ToInt32(tag, 16);
        }

        public int Tag { get;}
        public DecodeOption DecodeOption { get; set; }
    }

    public enum DecodeOption
    {
        Default,Hex,Ascii
    }
}