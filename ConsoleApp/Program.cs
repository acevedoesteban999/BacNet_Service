using BB_Service_Library;
using System.IO.BACnet;
using System.Diagnostics;

class ConsoleApp
{
    //static void Main()
    //{
    //    BacNet_Service Client_OR = new BacNet_Client_OnlyRead_Service(444);
    //    BacNet_Service Client_WR = new BacNet_Client_ReadWrite_Service(443);
    //    Client_OR.StartActivity();
    //    Client_WR.StartActivity();
    //    while (true)
    //    {
    //        Thread.Sleep(500);
    //        Console.Clear();
    //        Console.WriteLine(Client_OR.GetID() + ":" + Client_OR.GetValue());
    //        Console.WriteLine(Client_WR.GetID() + ":" + Client_WR.GetValue());
    //    }

    //}
    static void Main()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
        BacNet_Service Server = new BacNet_Server_Service(400);
        Server.StartActivity();

        Thread.Sleep(1000);
        BacnetValue _bacnetValueR;
        BacnetValue _bacnetValueW;
        while (true)
        {
            try
            {

                Console.WriteLine("ID?");
                int _id = Convert.ToInt32(Console.ReadLine());
                if (_id == 999)
                {
                    IList<BacnetValueID> Values;
                    Server.ReadAllScalarValue(out Values);
                    foreach (BacnetValueID bnvid in Values)
                        Console.WriteLine(bnvid.ID + ":" + bnvid.Value);
                }
                else
                {
                    Console.WriteLine("Value?");
                    _bacnetValueW = new BacnetValue(Convert.ToInt32(Console.ReadLine()));
                    Server.WriteScalarValue(_id, _bacnetValueW);
                }

                Console.WriteLine("Pulse una tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
            }
            catch (Exception e) { Console.WriteLine(e.Message); }
        }

    }
}