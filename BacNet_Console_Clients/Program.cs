using BacNet_ClassLibrary;

class BacNet_Console_Clients
{
    static void Main()
    {
        BacNet_Service Client_OR;
        BacNet_Service Client_WR;
        Console.WriteLine("Static ID?");
        while (true)
        {
            try
            {
                if (Console.ReadLine() != "static")
                {
                    Random r = new Random();
                    Client_OR = new BacNet_Client_OnlyRead((uint)r.Next(1, 1000));
                    Client_WR = new BacNet_Client_ReadWrite((uint)r.Next(1, 1000));

                }
                else
                {
                    Console.WriteLine("ID OnlyRead?");
                    Client_OR = new BacNet_Client_OnlyRead(Convert.ToUInt32(Console.ReadLine()));
                    Console.WriteLine("ID ReadWrite?");
                    Client_WR = new BacNet_Client_ReadWrite(Convert.ToUInt32(Console.ReadLine()));
                }
            }
            catch (Exception e) { Console.Clear(); continue; }
            break;
        }
        Console.WriteLine("StartActivity...");
        Client_OR.StartActivity();
        Client_WR.StartActivity();
        int  WR = 0;
        while (true)
        {
            Thread.Sleep(1000);
            if (WR != Client_WR.GetValue())
            {
                WR = Client_WR.GetValue();
                Console.Clear();
                Console.WriteLine(Client_OR.GetBacNetType().ToString() + "\nID:" + Client_OR.GetID() + "\nValue:" + Client_OR.GetValue());
                Console.WriteLine("\n");
                Console.WriteLine(Client_WR.GetBacNetType().ToString() + "\nID:" + Client_WR.GetID() + "\nValue:" + Client_WR.GetValue());
            }
        }
    }
}
