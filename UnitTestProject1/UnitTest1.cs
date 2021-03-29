using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TemplateNetworkFramework.Classes;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Inicializing()
        {
            var client = new NetClient("Michael", "123", null, null);
            var server = new NetServer(IPAddress.Parse("127.0.0.1"), 8888, "", null, null);//127.0.0.1
            server.StartServer();
            client.Connect("127.0.0.1", 8888);
            client.SendCommand<TemplateStringMessage>("uzer");
            client.Disconnect();
            server.StopServer();
        }
    }
}
