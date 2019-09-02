using System.Text.Json;
using System.Threading.Tasks;
using Kernel.Emv;
using Kernel.Tlv;
using Kernel.Utils;
using Moq;
using Xunit;

namespace Kernel.Tests
{
    public class TlvSerializerTests
    {
        [Fact]
        public void TestDeserialiser()
        {
            var tlvBytes = Helpers.HexStringToByteArray("6F1A840E315041592E5359532E4444463031A5088801015F2D02656E");
            var tlv = new Tlv.Tlv(tlvBytes);

            var result = TlvSerializer.Deserialize<Application>(tlv,"0x6f");
            Assert.NotNull(result);
        }

        [Fact]
        public void TestSerializer()
        {
            var tlvBytes = Helpers.HexStringToByteArray("6F1A840E315041592E5359532E4444463031A5088801015F2D02656E");
            var tlv = new Tlv.Tlv(tlvBytes);

            var result = TlvSerializer.Deserialize<Application>(tlv,"0x6f");
            Assert.NotNull(result);

            var tlv2 = TlvSerializer.Serialize(result);
            var result2 = TlvSerializer.Deserialize<Application>(tlv2,"0x6f");
            Assert.Equal( JsonSerializer.Serialize(result),JsonSerializer.Serialize(result2));
        }
        
        
    }
}