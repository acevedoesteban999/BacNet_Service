using System.IO.BACnet;
using System.Diagnostics;
using System.IO.BACnet.Storage;
namespace BB_Service_Library
{
    public struct BacnetValueID
    {
        public BacnetValueID(uint id, BacnetValue bnv)
        {
            Value = bnv;
            ID = id;
        }
        public BacnetValue Value;
        public uint ID;
    }
    public class BacNet_Server_Service : BacNet_Service
    {

        public BacNet_Server_Service(uint ID=255)
            : base(ID)
        {
            bacNetType = BacNetTypes.Server;
            value = new BacnetValue(0);
            BacnetValue[] NoScalarValue = { value };
            m_storage.WriteProperty(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, 1, NoScalarValue, true);

        }
        public override void StartActivity()
        {

            bacnet_client.Start();
            //bacnet_client.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            //bacnet_client.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            //bacnet_client.OnWritePropertyRequest += new BacnetClient.WritePropertyRequestHandler(handler_OnWritePropertyRequest);
            Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + "-> Stared! -> OnWhoIs");
            bacnet_client.WhoIs();
        }
        public override void ReadAllScalarValue(out IList<BacnetValueID> ValuesID)
        {
            ValuesID = new List<BacnetValueID>();
            IList<BacnetValue> NoScalarValue;
            foreach ( BacNode bn in DevicesList)
            {
                if(bacnet_client.ReadPropertyRequest(bn.adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, out NoScalarValue)==false)               
                    continue;    
                 ValuesID.Add(new BacnetValueID(bn.device_id,NoScalarValue[0]));          
            }
        }
    }
}
