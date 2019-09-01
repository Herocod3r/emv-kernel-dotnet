using Kernel.Utils;
using Xunit;

namespace Kernel.Tests
{
    public class TlvTests
    {
        [Fact]
        public void TestTlvDecode()
        {
            var tlvBytes = Helpers.HexStringToByteArray("9f2701009f360200419f2608c74d18b08248fefc9f10120110201009248400000000000000000029ff");
            var tlv = new Tlv.Tlv(); 
            tlv.Decode(tlvBytes);
            Assert.NotEmpty(tlv);
        }

        [Fact]
        public void TestTlvEncode()
        {
            var tlvBytes = Helpers.HexStringToByteArray("9f2701009f360200419f2608c74d18b08248fefc9f10120110201009248400000000000000000029ff");
            var tlv = new Tlv.Tlv(); 
            tlv.Decode(tlvBytes);

            var encoded = tlv.Encode();
            var tlv2 = new Tlv.Tlv();
            tlv2.Decode(encoded);

            Assert.Equal(tlvBytes, encoded);

        }
    }
}