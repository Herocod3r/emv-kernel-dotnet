namespace Kernel.Emv
{
    public class Terminal
    {
        public int Type { get; set; }
        public byte[] CountryCode { get; set; }
        public int CurrencyCode { get; set; }
    }
}