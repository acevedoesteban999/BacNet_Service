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
            BacnetValue[] NoScalarValue = { value };
            DeviceStorage.ErrorCodes code = m_storage.WriteProperty(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, 1, NoScalarValue, true);

        }
        public override void StartActivity()
        {

            bacnet_client.Start();
            bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            Console.WriteLine(bacNetType.ToString()+"-ID:" + m_storage.DeviceId + "-> Stared! -> OnWhoIs");
            bacnet_client.WhoIs();
        }
        

    }
}
