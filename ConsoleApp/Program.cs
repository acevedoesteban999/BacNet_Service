using BB_Service_Library;
using System.IO.BACnet;
using System.Diagnostics;

class ConsoleApp
{
    //static void Main()
    //{
    //    Random r = new Random();
    //    BacNet_Service Client_OR = new BacNet_Client_OnlyRead_Service((uint)r.Next(1, 1000));
    //    BacNet_Service Client_WR = new BacNet_Client_ReadWrite_Service((uint)r.Next(1, 1000));
    //    Client_OR.StartActivity();
    //    Client_WR.StartActivity();
    //    BacnetValue bnv0 = new BacnetValue(0), bnv1 = new BacnetValue(0);
    //    while (true)
    //    {
    //        if (bnv0.Value != Client_OR.GetValue().Value || bnv1.Value != Client_WR.GetValue().Value)
    //        {
    //            bnv0 = Client_OR.GetValue();
    //            bnv1 = Client_WR.GetValue();
    //            Console.Clear();
    //            Console.WriteLine(Client_OR.GetBacNetType().ToString() + "\nID:" + Client_OR.GetID() + "\nValue:" + Client_OR.GetValue());
    //            Console.WriteLine("\n");
    //            Console.WriteLine(Client_WR.GetBacNetType().ToString() + "\nID:" + Client_WR.GetID() + "\nValue:" + Client_WR.GetValue());
    //        }
    //    }
    //}
    static void Main()
    {
        BacNet_Server_Service Server = new BacNet_Server_Service(400);
        Server.StartActivity();

        Thread.Sleep(1000);
        //BacnetValue _bacnetValueR;
        //BacnetValue _bacnetValueW;
        int Vw;
        int ContDevice = 0;
        IList<uint> devices = new List<uint>();
        while (true)
        {
            try
            {
                if (Server.GetContDevices() != ContDevice)
                {
                    ContDevice = Server.GetContDevices();
                    Server.GetDevices(out devices);
                }
                Console.WriteLine("Devices:" + ContDevice);
                foreach (uint ui in devices)
                    Console.WriteLine(ui);

                Console.WriteLine("\nread? || write?");
                string s = Console.ReadLine();
                if (s == "read")
                {
                    Console.WriteLine("Reading All Device...");
                    IList<BacnetValueID> Values;
                    Server.ReadAllScalarValue(out Values);
                    foreach (BacnetValueID bnvid in Values)
                        Console.WriteLine(bnvid.ID + ":" + bnvid.Value);
                }
                else if (s == "write")
                {

                    

                    Console.WriteLine("ID?");
                    int _id = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Value?");
                    Vw = (Convert.ToInt32(Console.ReadLine()));
                    if (Server.WriteScalarValue(_id, Vw))
                        Console.WriteLine(_id + "=>" + Vw + " OK.");
                }
                else
                    Console.WriteLine("Command not valid:" + s);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Pulse una tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
        }

    }
}