namespace Kernel.Emv.Apdu
{
    public class APDUCommand
    {
        public byte Class { get; set; }
        public byte Instruction { get; set; }
        public byte Parameter1 { get; set; }
        public byte Parameter2 { get; set; }
        public byte[] Data { get; set; }
        public byte ExpectedLength { get; set; }
    }
}