namespace Kernel.Emv.Apdu
{
    public class APDUResponse
    {
        public byte[] Body { get; set; }
        public byte SW1 { get; set; }
        public byte SW2 { get; set; }
    }
}