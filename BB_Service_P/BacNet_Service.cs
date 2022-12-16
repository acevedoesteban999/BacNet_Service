using System.IO.BACnet;
using System.Diagnostics;
using System.IO.BACnet.Storage;

namespace BB_Service_Library
{
    public class BacNode
    {
        BacnetAddress adr;
        uint device_id;

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
        public BacNet_Service(uint ID=255)
        {
            bacnet_client = new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));
            DevicesList = new List<BacNode>();
            m_storage = new DeviceStorage();
            m_storage.DeviceId = ID;
            BacnetValue[] NoScalarValue = { value };
            DeviceStorage.ErrorCodes code = m_storage.WriteProperty(new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, 1, NoScalarValue, true);

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
            
            if (bacnet_client.ReadPropertyRequest(adr, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, out NoScalarValue) == false)
                return false;

            Value = NoScalarValue[0];
            return true;
        }

        /*****************************************************************************************************/
        public bool WriteScalarValue(int device_id, BacnetValue Value)
        {
            if (this.bacNetType == BacNetTypes.Client_OnlyRead)
                return false;
            BacnetAddress adr;

            // Looking for the device
            adr = FindDeviceAddr((uint)device_id);
            if (adr == null) return false;  // not found

            // Property Write
            BacnetValue[] NoScalarValue = { Value };
            BacnetObjectId bb = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);
            if (bacnet_client.WritePropertyRequest(adr, bb, BacnetPropertyIds.PROP_PRESENT_VALUE, NoScalarValue) == false)
                return false;

            return true;
        }
        /*****************************************************************************************************/

        //protected void ReadWriteExample()
        //{

        //    BacnetValue Value;
        //    bool ret;
        //    // Read Present_Value property on the object ANALOG_INPUT:0 provided by the device 12345
        //    // Scalar value only
        //    ret = ReadScalarValue(12345, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_INPUT, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, out Value);

        //    if (ret == true)
        //    {
        //        Console.WriteLine("Read value : " + Value.Value.ToString());

        //        // Write Present_Value property on the object ANALOG_OUTPUT:0 provided by the device 4000
        //        BacnetValue newValue = new BacnetValue(Convert.ToSingle(Value.Value));   // expect it's a float
        //        ret = WriteScalarValue(4000, new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_OUTPUT, 0), BacnetPropertyIds.PROP_PRESENT_VALUE, newValue);

        //        Console.WriteLine("Write feedback : " + ret.ToString());
        //    }
        //    else
        //        Console.WriteLine("Error somewhere !");
        //}

        /*****************************************************************************************************/
        public void handler_OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            Console.WriteLine(bacNetType.ToString()+"-ID:" + m_storage.DeviceId + "-> OnWhoIs");
            if (low_limit != -1 && m_storage.DeviceId < low_limit) return;
            else if (high_limit != -1 && m_storage.DeviceId > high_limit) return;
            sender.Iam(m_storage.DeviceId, new BacnetSegmentations());
        }
        /*****************************************************************************************************/
        public void handler_OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {
            Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + (device_id!= m_storage.DeviceId?"-> OnIam":"-> Auto_Iam"));
            lock (DevicesList)
            {
                // Device already registred ?
                foreach (BacNode bn in DevicesList)
                    if (bn.getAdd(device_id) != null) return;   // Yes

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
            Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + "-> OnReadPropertyRequest");
            lock (m_storage)
            {
                try
                {
                    IList<BacnetValue> BacNetIList;
                    DeviceStorage.ErrorCodes code = m_storage.ReadProperty(object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, out BacNetIList);

                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
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
            Console.WriteLine(bacNetType.ToString() + "-ID:" + m_storage.DeviceId + "-> OnWritePropertyRequest");
            lock (m_storage)
            {
                try
                {
                    DeviceStorage.ErrorCodes code = m_storage.WriteProperty(object_id, BacnetPropertyIds.PROP_PRESENT_VALUE, 1, value.value, true);
                    if (code == DeviceStorage.ErrorCodes.NotForMe)
                        code = m_storage.WriteProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.property.propertyArrayIndex, value.value);

                    if (code == DeviceStorage.ErrorCodes.Good)
                    {
                        sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id);
                        this.value = new BacnetValue(value.value);
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
    }
}
