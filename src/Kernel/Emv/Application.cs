using Kernel.Tlv;

namespace Kernel.Emv
{
    public class Application
    {
        [TlvProperty("84")]
        public byte[] DedicatedFileName { get; set; }

        [TlvProperty("a5")]
        public ProprietaryTemplate Template { get; set; }
    }

    public class ProprietaryTemplate
    {
        [TlvProperty("88")]
        public int Sfi { get; set; }
        [TlvProperty("50")]
        public string Label { get; set; }
        [TlvProperty("87")]
        public int Priority { get; set; }
        [TlvProperty("5f2d")]
        public string LanguagePreference { get; set; }
        [TlvProperty("bf0c")]
        public byte[] DiscretionaryData { get; set; }
    }
}