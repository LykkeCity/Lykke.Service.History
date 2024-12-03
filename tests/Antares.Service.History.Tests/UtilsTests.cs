using System;
using Antares.Service.History.Core;
using Xunit;

namespace Antares.Service.History.Tests
{
    public class UtilsTests
    {
        [Fact]
        public void IncrementArray_Test()
        {
            Assert.Equal(new byte[] { 0, 45, 0 }, Utils.IncrementByteArray(new byte[] { 0, 0, 0 }, 300));

            Assert.Equal(new byte[] { 253, 252, 255 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 1));

            Assert.Equal(new byte[] { 254, 0, 0 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 5));

            Assert.Equal(new byte[] { 0, 0, 0 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 100));

            Assert.Equal(new byte[] { 253, 252, 254 }, Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, 0));

            Assert.Throws<ArgumentOutOfRangeException>(() => Utils.IncrementByteArray(new byte[] { 253, 252, 254 }, -10));
        }

        [Theory]
        [InlineData("ConsoleInitiated-518d58a0-9f75-4c25-b1c6-85d3251fdfb5", "518d58a0-9f75-4c25-b1c6-85d3251fdfb5")]
        [InlineData("618d58a0-9f75-4c25-b1c6-85d3251fdfb5", "618d58a0-9f75-4c25-b1c6-85d3251fdfb5")]
        [InlineData("a0-9f75-4c25-b1c6-85d3251fdfb5", "")]
        [InlineData("101215", "")]
        [InlineData("618d58a0-9f75-4c25-b1c6-85d3251fdfb5-asdsadsadsad", "618d58a0-9f75-4c25-b1c6-85d3251fdfb5")]
        public void GuidExtract_Test(string str, string expected)
        {
            var result = "";

            if (Utils.TryExtractGuid(str, out var guid))
                result = guid.ToString();

            Assert.Equal(expected, result);
        }
    }
}
