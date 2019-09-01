using Kernel.Utils;
using Xunit;

namespace Kernel.Tests
{
    public class HelpersTest
    {
        [Fact]
        public void TestArraySlice()
        {
            var arr = new string[] {"jethro","Daniel","Mike" };
            var rsp = Helpers.ArraySlice(arr, 1, arr.Length);
            Assert.Equal(2,rsp.Length);

        }
    }
}