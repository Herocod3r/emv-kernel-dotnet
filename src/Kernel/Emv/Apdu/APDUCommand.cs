namespace Kernel.Emv.Apdu
{
    public class APDUCommand
    {
        public byte Class { get; set; } = 0x00;
        public byte Instruction { get; set; }
        public byte Parameter1 { get; set; } = 0x00;
        public byte Parameter2 { get; set; } = 0x00;
        public byte[] Data { get; set; }
        public byte ExpectedLength { get; set; } = 0x00;
    }
}