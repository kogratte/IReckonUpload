using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace IReckonUpload.Tools.Tests
{
    [TestClass]
    public class Sha256BuilderTests
    {
        [TestMethod]
        public void ItShouldReturnTheSha256FromKnownString()
        {
            Sha256Builder.Compute("password").ShouldBe("5E884898DA28047151D0E56F8DC6292773603D0D6AABBDD62A11EF721D1542D8");
            Sha256Builder.Compute("secret").ShouldBe("2BB80D537B1DA3E38BD30361AA855686BDE0EACD7162FEF6A25FE97BF527A25B");
        }
    }
}
