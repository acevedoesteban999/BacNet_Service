using BB_Service_Library;
using System.IO.BACnet;
using System.Diagnostics;

class ConsoleApp
{
    static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());


        BacNet_Service Server = new BacNet_Service(444);
      
        Server.StartActivity();

        Thread.Sleep(1000);
        BacnetObjectId OBJECT_ANALOG_VALUE_0 = new BacnetObjectId(BacnetObjectTypes.OBJECT_ANALOG_VALUE, 0);


        BacnetPropertyIds bacnetPropertyIds = BacnetPropertyIds.PROP_PRESENT_VALUE;
        BacnetValue _bacnetValue= new BacnetValue(24);

        while (true)
        {
            char a = Console.ReadKey().KeyChar;
            if (a=='w')
            {
                Server.WriteScalarValue(444, OBJECT_ANALOG_VALUE_0, bacnetPropertyIds, _bacnetValue);

            }
            else  if(a=='r')
            {
                _bacnetValue.Value = 0;
                Server.ReadScalarValue(444, OBJECT_ANALOG_VALUE_0, bacnetPropertyIds, out _bacnetValue);
                Console.WriteLine("Value:" + _bacnetValue);
            }
        }

        
        
        
        
        //bb_service.Start();
        

    }
}