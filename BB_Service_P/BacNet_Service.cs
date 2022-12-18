using System.IO.BACnet;
using System.IO.BACnet.Storage;

namespace BB_Service_Library
{
    public class BacNode
    {
        public BacnetAddress adr;
        public uint device_id;

        public BacNode(BacnetAddress adr, uint device_id)
        {
            this.adr = adr;
            this.device_id = device_id;
        }

        public BacnetAddress getAdd(uint device_id)
        {
            if (this.device_id == device_id)
                return adr;
            else
                return null;
        }
    }
    abstract public class BacNet_Service:BacNet_Interface
    {
        /*****************************************************************************************************/
        protected BacnetClient bacnet_client;
        protected DeviceStorage m_storage;
        protected List<BacNode> DevicesList;
        protected bool OnlyReadOneTime, OnlyWriteOneTime;
        protected BacnetValue value;
        protected BacNetTypes bacNetType;
        /*****************************************************************************************************/
        public BacNet_Service(uint ID)
        {
            bacnet_client=new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));
            DevicesList = new List<BacNode>();
            m_storage = new DeviceStorage();
            m_storage.DeviceId = ID;
            
        }
        /*****************************************************************************************************/
        public abstract void StartActivity();
        /*****************************************************************************************************/
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
            catch(Exception e)
            {
                if (e.Message.Contains("RECOGNIZED_SERVICE"))
                    Console.WriteLine("Exception: Reading not allowed!");
                else
                    Console.WriteLine("Exception:" + e.Message);
                return false;
            }
        }
        /*****************************************************************************************************/
        public bool WriteScalarValue(int device_id, BacnetValue Value)
        {
            BacnetAddress adr;

            // Looking for the device
            adr =FindDeviceAddr((uint)device_id);
            if (adr == null) return false;  // not found

            // Property Write
            BacnetValue[] NoScalarValue = { Value };
            BacnetObjectId bb = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);
            try 
            {
                if (bacnet_client.WritePropertyRequest(adr, bb, BacnetPropertyIds.PROP_PRESENT_VALUE, NoScalarValue) == false)
                    return false;

            }catch(Exception e)
            {
                if (e.Message.Contains("RECOGNIZED_SERVICE"))
                    Console.WriteLine("Exception: Writing not allowed!");
                else
                    Console.WriteLine("Exception:" + e.Message);
                return false;
            }



            return true;
        }
        /*****************************************************************************************************/
        public void handler_OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            if (low_limit != -1 && m_storage.DeviceId < low_limit) return;
            else if (high_limit != -1 && m_storage.DeviceId > high_limit) return;
            sender.Iam(m_storage.DeviceId, new BacnetSegmentations());
        }
        /*****************************************************************************************************/
        public void handler_OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {
            lock (DevicesList)
            {
                // Device already registred ?
                foreach (BacNode bn in DevicesList)
                    if (bn.getAdd(device_id) != null) return;   // Yes

                Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + (device_id != m_storage.DeviceId ? "-> OnIam::"+ device_id : "AutoIam"));
                // Not already in the list
                DevicesList.Add(new BacNode(adr, device_id));   // add it
            }
        }
        /*****************************************************************************************************/
        public void handler_OnReadPropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyReference property, BacnetMaxSegments max_segments)
        {
            if (!OnlyReadOneTime)
                OnlyReadOneTime = true;
            else
                return;
            lock (m_storage)
            {
                try
                {
                    IList<BacnetValue> BacNetIList;
                    DeviceStorage.ErrorCodes code = m_storage.ReadProperty(object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, out BacNetIList);

                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
                        //Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + "-> OnReadPropertyRequest");
                        sender.ReadPropertyResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), object_id, property, BacNetIList);
                        value = BacNetIList[0];
                    }
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
            OnlyReadOneTime = false;
        }
        /*****************************************************************************************************/
        public void handler_OnWritePropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyValue value, BacnetMaxSegments max_segments)
        {
            if (!OnlyWriteOneTime)
                OnlyWriteOneTime = true;
            else
                return;
            if (this.bacNetType == BacNetTypes.Client_OnlyRead)
                return;
            lock (m_storage)
            {
                try
                {
                    DeviceStorage.ErrorCodes code = m_storage.WriteProperty(object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, value.value, true);
                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
                        //Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + "-> OnWritePropertyRequest");
                        sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id);
                        this.value = value.value[0];
                    }
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                
            }
            OnlyWriteOneTime = false;
        }
        /*****************************************************************************************************/
        protected BacnetAddress FindDeviceAddr(uint device_id)
        {
            BacnetAddress ret;

            lock (DevicesList)
            {
                foreach (BacNode bn in DevicesList)
                {
                    ret = bn.getAdd(device_id);
                    if (ret != null) return ret;
                }
                // not in the list
                return null;
            }
        }
        public BacnetValue GetValue() { return this.value; }
        public uint GetID() { return this.m_storage.DeviceId; }
        public virtual void ReadAllScalarValue(out  IList<BacnetValueID> ValuesID) { ValuesID = new List<BacnetValueID>();}
    }
}
