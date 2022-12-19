using System.IO.BACnet;
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
        public BacNet_Server_Service(uint ID)
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
            bacnet_client.OnIam += new BacnetClient.IamHandler(handler_OnIam);
            bacnet_client.WhoIs();
        }
        public void ReadAllScalarValue(out IList<BacnetValueID> ValuesID)
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
        public bool ReadScalarValue(int device_id, out int Value)
        {
            BacnetValue bv;
            bool  a= ReadScalarValue(device_id,out bv);
            Value=Convert.ToInt32(bv.Value.ToString());
            return a;
        }
        public bool ReadScalarValue(int device_id, out BacnetValue Value)
        {

            BacnetAddress adr;
            IList<BacnetValue> NoScalarValue;
            Value = new BacnetValue(null);

            adr = FindDeviceAddr((uint)device_id);
            if (adr == null) return false;  // not found

            // Property Read
            try
            {
                if (bacnet_client.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, out NoScalarValue) == false)
                    return false;
                Value = NoScalarValue[0];
                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("RECOGNIZED_SERVICE"))
                    Console.WriteLine("Exception: Reading not allowed!");
                else
                    Console.WriteLine("Exception:" + e.Message);
                return false;
            }
        }
        public bool WriteScalarValue(int device_id, int Value)
        {
            return WriteScalarValue(device_id, new BacnetValue(Value));
        }
        public bool WriteScalarValue(int device_id, BacnetValue Value)
        {
            BacnetAddress adr;

            // Looking for the device
            adr = FindDeviceAddr((uint)device_id);
            if (adr == null) return false;  // not found

            // Property Write
            BacnetValue[] NoScalarValue = { Value };
            BacnetObjectId bb = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);
            try
            {
                if (bacnet_client.WritePropertyRequest(adr, bb, BacnetPropertyIds.PROP_PRESENT_VALUE, NoScalarValue) == false)
                    return false;

            }
            catch (Exception e)
            {
                if (e.Message.Contains("RECOGNIZED_SERVICE"))
                    Console.WriteLine("Exception: Writing not allowed!");
                else
                    Console.WriteLine("Exception:" + e.Message);
                return false;
            }



            return true;
        } 
    }
   

}
