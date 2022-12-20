using BacNet_ClassLibrary;

class BacNet_Console_Server
{
    static void Main()
    {
        BacNet_Server Server = new BacNet_Server(400);
        Server.StartActivity();
        Thread.Sleep(1000);
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
