using BB_Service_Library;
using System.IO.BACnet;
using System.Diagnostics;

class ConsoleApp
{
    static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        BacNet_Service Server = new BacNet_Server_Service(400);
        BacNet_Service Client_OR = new BacNet_Client_OnlyRead_Service(444);
        BacNet_Service Client_WR = new BacNet_Client_ReadWrite_Service(443);
        Server.StartActivity();
        Client_OR.StartActivity();
        Client_WR.StartActivity();

        Thread.Sleep(1000);
        BacnetValue _bacnetValueR;
        BacnetValue _bacnetValueW; 

        while (true)
        {
            char a = Console.ReadKey().KeyChar;
            if (a == 'q')
            {
                _bacnetValueW = new BacnetValue(88);
                Server.WriteScalarValue(443, _bacnetValueW);

            }
            else if (a=='w')
            {
                _bacnetValueW = new BacnetValue(89);
                Server.WriteScalarValue(444, _bacnetValueW);
                
            }
            else  if(a=='r')
            {
                IList<BacnetValueID> Values;
                Server.ReadAllScalarValue(out Values);
                foreach (BacnetValueID bnvid in Values)
                    Console.WriteLine(bnvid.ID+":"+ bnvid.Value);

            }
        }

        
        
        
        
        //bb_service.Start();
        

    }
}