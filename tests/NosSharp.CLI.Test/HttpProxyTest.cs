using Microsoft.VisualStudio.TestTools.UnitTesting;
using NosSharp.CLI.Interfaces;
using NosSharp.CLI.Proxies;

namespace NosSharp.CLI.Test
{
    [TestClass]
    public class HttpProxyTest
    {
        private readonly ICliProxy _proxy = new HttpCliProxy();

        [TestMethod]
        public void TestHttpProxyConnect()
        {
            Assert.IsFalse(_proxy.Connect("localhost", 80));
        }
    }
}
