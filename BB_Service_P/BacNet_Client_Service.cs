using System.IO.BACnet;
using System.Diagnostics;
using System.IO.BACnet.Storage;
namespace BB_Service_Library
{
    public class BacNet_Client_Service:BacNet_Service
    {
        public BacNet_Client_Service(uint ID)
            :base(ID)
        {

        }
        public override void StartActivity()
        {

            bacnet_client.Start();
            bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            bacnet_client.OnReadPropertyMultipleRequest += new BacnetClient.ReadPropertyMultipleRequestHandler(handler_OnReadPropertyMultipleRequest);
            bacnet_client.OnWritePropertyRequest += new BacnetClient.WritePropertyRequestHandler(handler_OnWritePropertyRequest);
            Console.WriteLine("ID:" + m_storage.DeviceId + "-> Stared!");

            bacnet_client.WhoIs();
        }
    }
}
