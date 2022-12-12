using System.IO.BACnet;
using System.IO.BACnet.Storage;

namespace BB_Service_Library
{
    class BacNet_Service:BacNet_Interface
    {
        private BacnetClient bacnet_server;
        private DeviceStorage m_storage;
        
        public void StartActivity()
        {
            // Load the device descriptor from the embedded resource file
            // Get myId as own device id

            m_storage = new DeviceStorage();
            m_storage.DeviceId = 79;
            //m_storage = DeviceStorage.Load("BasicServer.DeviceDescriptor.xml");

            // Bacnet on UDP/IP/Ethernet

            bacnet_server = new BacnetClient(new BacnetIpUdpProtocolTransport(0xBAC0, false));


            // or Bacnet Mstp on COM4 à 38400 bps, own master id 8
            // m_bacnet_server = new BacnetClient(new BacnetMstpProtocolTransport("COM4", 38400, 8);
            // Or Bacnet Ethernet
            // bacnet_server = new BacnetClient(new BacnetEthernetProtocolTransport("Connexion au réseau local"));    
            // Or Bacnet on IPV6
            // bacnet_server = new BacnetClient(new BacnetIpV6UdpProtocolTransport(0xBAC0));

            bacnet_server.OnWhoIs += new BacnetClient.WhoIsHandler(handler_OnWhoIs);
            bacnet_server.OnIam += new BacnetClient.IamHandler(bacnet_server_OnIam);
            bacnet_server.OnReadPropertyRequest += new BacnetClient.ReadPropertyRequestHandler(handler_OnReadPropertyRequest);
            bacnet_server.OnReadPropertyMultipleRequest += new BacnetClient.ReadPropertyMultipleRequestHandler(handler_OnReadPropertyMultipleRequest);
            bacnet_server.OnWritePropertyRequest += new BacnetClient.WritePropertyRequestHandler(handler_OnWritePropertyRequest);

            bacnet_server.Start();    // go
            Console.WriteLine("BacNet_Service Stared!");
            // Send Iam
            bacnet_server.Iam(m_storage.DeviceId, new BacnetSegmentations());
            
        }

        public void bacnet_server_OnIam(BacnetClient sender, BacnetAddress adr, uint device_id, uint max_apdu, BacnetSegmentations segmentation, ushort vendor_id)
        {
            Console.WriteLine("bacnet_OnIam:\nIgnored Iam...");
            //ignore Iams from other devices. (Also loopbacks)
        }
        public void handler_OnWhoIs(BacnetClient sender, BacnetAddress adr, int low_limit, int high_limit)
        {
            Console.WriteLine("handler_OnWhoIs");
            if (low_limit != -1 && m_storage.DeviceId < low_limit) return;
            else if (high_limit != -1 && m_storage.DeviceId > high_limit) return;
            sender.Iam(m_storage.DeviceId, new BacnetSegmentations());
        }

        /*****************************************************************************************************/
        public void handler_OnWritePropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyValue value, BacnetMaxSegments max_segments)
        {
            Console.WriteLine("handler_OnWritePropertyRequest");

            // only OBJECT_ANALOG_VALUE:0.PROP_PRESENT_VALUE could be write in this sample code
            if ((object_id.type != BacnetObjectTypes.OBJECT_ANALOG_VALUE) || (object_id.instance != 0) || ((BacnetPropertyIds)value.property.propertyIdentifier != BacnetPropertyIds.PROP_PRESENT_VALUE))
            {
                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_WRITE_ACCESS_DENIED);
                return;
            }

            lock (m_storage)
            {
                try
                {
                    DeviceStorage.ErrorCodes code = m_storage.WriteCommandableProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.value[0], value.priority);
                    if (code == DeviceStorage.ErrorCodes.NotForMe)
                        code = m_storage.WriteProperty(object_id, (BacnetPropertyIds)value.property.propertyIdentifier, value.property.propertyArrayIndex, value.value);

                    if (code == DeviceStorage.ErrorCodes.Good)
                        sender.SimpleAckResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id);
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_WRITE_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }
        /*****************************************************************************************************/
        
        /*****************************************************************************************************/
        public void handler_OnReadPropertyRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, BacnetObjectId object_id, BacnetPropertyReference property, BacnetMaxSegments max_segments)
        {
            Console.WriteLine("handler_OnReadPropertyRequest");
            lock (m_storage)
            {
                try
                {
                    IList<BacnetValue> value;
                    DeviceStorage.ErrorCodes code = m_storage.ReadProperty(object_id, (BacnetPropertyIds)property.propertyIdentifier, property.propertyArrayIndex, out value);
                    if (code == DeviceStorage.ErrorCodes.Good)
                        sender.ReadPropertyResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), object_id, property, value);
                    else
                        sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROPERTY, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }

        /*****************************************************************************************************/
        public void handler_OnReadPropertyMultipleRequest(BacnetClient sender, BacnetAddress adr, byte invoke_id, IList<BacnetReadAccessSpecification> properties, BacnetMaxSegments max_segments)
        {
            Console.WriteLine("handler_OnReadPropertyMultipleRequest");
            lock (m_storage)
            {
                try
                {
                    IList<BacnetPropertyValue> value;
                    List<BacnetReadAccessResult> values = new List<BacnetReadAccessResult>();
                    foreach (BacnetReadAccessSpecification p in properties)
                    {
                        if (p.propertyReferences.Count == 1 && p.propertyReferences[0].propertyIdentifier == (uint)BacnetPropertyIds.PROP_ALL)
                        {
                            if (!m_storage.ReadPropertyAll(p.objectIdentifier, out value))
                            {
                                sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_OBJECT, BacnetErrorCodes.ERROR_CODE_UNKNOWN_OBJECT);
                                return;
                            }
                        }
                        else
                            m_storage.ReadPropertyMultiple(p.objectIdentifier, p.propertyReferences, out value);
                        values.Add(new BacnetReadAccessResult(p.objectIdentifier, value));
                    }

                    sender.ReadPropertyMultipleResponse(adr, invoke_id, sender.GetSegmentBuffer(max_segments), values);

                }
                catch (Exception)
                {
                    sender.ErrorResponse(adr, BacnetConfirmedServices.SERVICE_CONFIRMED_READ_PROP_MULTIPLE, invoke_id, BacnetErrorClasses.ERROR_CLASS_DEVICE, BacnetErrorCodes.ERROR_CODE_OTHER);
                }
            }
        }
    }
}
