using BB_Service_Library;
using System.IO.BACnet;
using System.Diagnostics;

class ConsoleApp
{
    static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        BacNet_Service Client_OR = new BacNet_Client_OnlyRead_Service(444);
        BacNet_Service Client_OR1 = new BacNet_Client_OnlyRead_Service(443);

        Client_OR.StartActivity();
        Client_OR1.StartActivity();

        Thread.Sleep(1000);

        BacnetValue _bacnetValue= new BacnetValue(24);

        while (true)
        {
            char a = Console.ReadKey().KeyChar;
            if (a=='w')
            {
                Client_OR.WriteScalarValue(444, _bacnetValue);

            }
            else  if(a=='r')
            {
                _bacnetValue.Value = 0;
                if(Client_OR.ReadScalarValue(444, out _bacnetValue))
                    Console.WriteLine("Value:" + _bacnetValue);
                if (Client_OR.ReadScalarValue(443, out _bacnetValue))
                    Console.WriteLine("Value:" + _bacnetValue);

            }
        }

        
        
        
        
        //bb_service.Start();
        

    }
}