using BB_Service_Library;

class ConsoleApp
{
    static void Main()
    {
        BB_Service bb_service=new BB_Service();
        bb_service.Start();
        while (true) ;

    }
}