using Kernel.Tlv;

namespace Kernel.Emv
{
    public class ApplicationInformation
    {
        [TlvProperty("4F")]
        public byte[] Name { get; set; }
        [TlvProperty("50")]
        public string Label { get; set; }
        [TlvProperty("87")]
        public int Priority { get; set; }
    }
}