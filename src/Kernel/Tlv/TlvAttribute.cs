using System;

namespace Kernel.Tlv
{
    public class TlvPropertyAttribute : Attribute
    {
        public TlvPropertyAttribute(byte tag)
        {
            this.Tag = tag;
        }

        public byte Tag { get;}
        public DecodeOption DecodeOption { get; set; }
    }

    public enum DecodeOption
    {
        Default,Hex,Ascii
    }
}