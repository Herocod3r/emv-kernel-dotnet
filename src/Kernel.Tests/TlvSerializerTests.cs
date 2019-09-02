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

            var result = TlvSerializer.Deserialize<CardApplication>(tlv).App;
            Assert.NotNull(result);
        }
        
        
    }
}