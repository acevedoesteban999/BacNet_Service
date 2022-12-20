using System.IO.BACnet;
namespace BacNet_ClassLibrary
{
    public class BacNet_Client_ReadWrite : BacNet_Service
    {
        public BacNet_Client_ReadWrite(uint ID)
            : base(ID)
        {
            bacNetType = BacNetTypes.Client_ReadWrite;
            Random r = new Random();
            value = new BacnetValue(r.Next(0, 100));
            BacnetValue[] NoScalarValue = { value };
            m_storage.WriteProperty(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, 1, NoScalarValue, true);

        }
        public override void StartActivity()
        {
            bacnet_client.Start();
            bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            bacnet_client.OnWritePropertyRequest += new BacnetClient.WritePropertyRequestHandler(handler_OnWritePropertyRequest);
            bacnet_client.WhoIs();
        }
    }
}
