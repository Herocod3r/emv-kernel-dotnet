using Kernel.Tlv;

namespace Kernel.Emv
{
    public class CardInformation
    {
        [TlvProperty("5A",DecodeOption = DecodeOption.Hex)]
        public string Pan { get; set; }
        [TlvProperty("5F34")]
        public int SequenceNumber { get; set; }
        [TlvProperty("5F24",DecodeOption = DecodeOption.Hex)]
        public string ExpiryDate { get; set; }
        [TlvProperty("5F20")]
        public string HolderName { get; set; }
        [TlvProperty("57",DecodeOption = DecodeOption.Hex)]
        public string Track2 { get; set; }
        [TlvProperty("8C")]
        public DataObjectList RiskManagmentData { get; set; }
        [TlvProperty("8F")]
        public int SchemePublicKeyIndex { get; set; }
        [TlvProperty("90")]
        public byte[] IssuerPublicKeyCertificate { get; set; }
        [TlvProperty("92")]
        public byte[] IssuerPublicKeyRemainder { get; set; }
        [TlvProperty("9F32")]
        public byte[] IssuerPublicKeyExponent { get; set; }
        [TlvProperty("9F46")]
        public byte[] IccPublicKeyCertificate { get; set; }
        [TlvProperty("9F48")]
        public byte[] IccPublicKeyRemainder { get; set; }
        [TlvProperty("93")]
        public byte[] SignedStaticApplicationData { get; set; }
        [TlvProperty("9F4A")]
        public TagList SdaTags { get; set; }
        [TlvProperty("0")]
        public Tlv.Tlv Raw { get; set; }
    }
}