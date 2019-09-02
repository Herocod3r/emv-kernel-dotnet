using Kernel.Tlv;

namespace Kernel.Emv
{
    public class ProcessingOptions
    {
        [TlvProperty("82")]
        public int ApplicationInterchangeProfile { get; set; }
        [TlvProperty("94")]
        public ApplicationFileList ApplicationFileList { get; set; }
        [TlvProperty("0")]
        public Tlv.Tlv Raw { get; set; }
    }
}