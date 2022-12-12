namespace BB_Service_Library
{
    public class BB_Service
    {
        private BacNet_Service _bacNetService=new BacNet_Service();

        public void Start()
        {
            _bacNetService.StartActivity();
        }
    }
}