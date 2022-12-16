using System.IO.BACnet;
using System.Diagnostics;
using System.IO.BACnet.Storage;
namespace BB_Service_Library
{
    public class BacNet_Client_OnlyRead_Service:BacNet_Service
    {
        
        public BacNet_Client_OnlyRead_Service(uint ID)
            :base(ID)
        {
            bacNetType = BacNetTypes.Client_OnlyRead;
            Random r = new Random();
            value=new BacnetValue(r.Next(0, 100));

        }
        public override void StartActivity()
        {

            bacnet_client.Start();
            bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            Console.WriteLine(bacNetType.ToString()+"-ID:" + m_storage.DeviceId + "-> Stared!");
            bacnet_client.WhoIs();
        }
        

    }
}
