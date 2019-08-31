using System;
using System.Threading.Tasks;
using Kernel.Emv;
using Kernel.Emv.Apdu;
using Moq;
using Xunit;

namespace Kernel.Tests
{
    public class CardTests
    {
        private ICardChip mockedCardChip;
        public CardTests()
        {
            var mock = new Mock<ICardChip>();
            mock.Setup(x => x.CardId).Returns(() => new byte[]{0x00,0x00,0x10,0x56});
            mock.Setup(x => x.TransmitCommandAsync(It.IsAny<byte[]>())).Returns(Task.FromResult(new byte[]{0x90,0x00}));
            mock.Setup(x => x.TransmitCommandAsync(It.Is<byte[]>(bytes => bytes.Length == 7)))
                .Returns(Task.FromResult(new byte[] {0x00, 0x00, 0x90, 0x00}));
            mock.Setup(x=>x.TransmitCommandAsync(It.Is<byte[]>(bytes => bytes.Length == 6))).Returns(Task.FromResult<byte[]>(null));
            mockedCardChip = mock.Object;
        }
        
        [Fact]
        public void Test_CardSendSuccessful_EmptyBody()
        {
            var card = new Card(mockedCardChip);
            var result = card.SendApduAsync(new APDUCommand {Instruction = 0xA4}).Result;
            Assert.NotNull(result);
            Assert.Null(result.Body);
            Assert.Equal(0x90,result.SW1);
            Assert.Equal(0x00,result.SW2);
        }

        [Fact]
        public void Test_CardSendSuccessful_Body()
        {
            
            var card = new Card(mockedCardChip);
            var result = card.SendApduAsync(new APDUCommand {Instruction = 0xA4,Data = new byte[]{0x10,0x00}}).Result;
            Assert.NotNull(result);
            Assert.NotNull(result.Body);
            Assert.Equal(0x90,result.SW1);
            Assert.Equal(0x00,result.SW2);
        }


        [Fact]
        public void Test_CardSendNullResponse()
        {
            var card = new Card(mockedCardChip);
            Assert.ThrowsAsync<APDUException>(async ()=> await card.SendApduAsync(new APDUCommand {Instruction = 0xA4,Data = new byte[]{0x10}}));

        }
    }
}