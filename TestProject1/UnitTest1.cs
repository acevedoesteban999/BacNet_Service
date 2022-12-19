using Microsoft.VisualStudio.TestTools.UnitTesting;
using BB_Service_Library;
namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        BacNet_Server_Service bnss = new BacNet_Server_Service(145);
        //BacNet_Client_ReadWrite_Service bnrws = new BacNet_Client_ReadWrite_Service(222);
        [TestMethod]
        public void TestMethod1()
        {
            bnss.StartActivity();
            //bnrws.StartActivity();
            //bnss.WriteScalarValue(222,22);
            //Assert.Equals(22, bnrws.GetValue().Value);
            int i;
            bnss.ReadScalarValue(234,out i);
            Assert.Equals(73, i);
        }
    }
}