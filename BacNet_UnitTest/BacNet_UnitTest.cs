using Microsoft.VisualStudio.TestTools.UnitTesting;
using BacNet_ClassLibrary;
using System.Collections.Generic;
using System.Threading;
namespace BacNet_UnitTest
{
    [TestClass]
    public class BacNet_UnitTest
    {
        BacNet_Client_ReadWrite Client=new BacNet_Client_ReadWrite(11);
        BacNet_Server Server=new BacNet_Server(22);
        [TestMethod]
        public void BacNetTestMethod()
        {
            Client.StartActivity();
            Server.StartActivity();
            Thread.Sleep(1000);
            IList <uint> lid;
            Server.GetDevices(out lid);
            uint device=0;
            foreach (uint ui in lid)
                device = ui;
            Assert.AreEqual(device, (uint)11);
            //Write in Client
            Server.WriteScalarValue(11, 37);
            //Check in Client
            int ClientValue =Client.GetValue();
            Assert.AreEqual(ClientValue, (int)37);
            //Check in Client by Protocol
            Server.ReadScalarValue(11, out ClientValue);
            Assert.AreEqual(ClientValue, (int)37);
        }
    }
}