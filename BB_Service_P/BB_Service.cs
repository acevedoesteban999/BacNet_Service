namespace BB_Service_Library
{
    public class BB_Service
    {
        private BacNet_Server_Service _bacNetServerService=new BacNet_Server_Service();
        //private Bluetooth BluetoothService;
        public void StartActivity()
        {
            _bacNetServerService.StartActivity();
            //BluetoothService.StartActivity();
        }
    }
}