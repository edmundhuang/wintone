using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using WintoneLib.Passports;

namespace WintoneConsole
{
    public class App
    {
        private readonly ReaderManager _readerManager;
        private readonly ILogger<App> _logger;

        public App(ReaderManager readerManager, ILogger<App> logger)
        {
            _readerManager = readerManager;
            _logger = logger;
        }

        public void Run()
        {
            InitReader();

            bool running = true;
            while(running) 
            {
                Console.WriteLine("Press X to exit, any key to continue.");
                var key = Console.ReadKey();

                switch(key.Key)
                {
                    case ConsoleKey.X:
                        running=false;
                        break;
                    default:
                        break;
                }

                Scan();

                Thread.Sleep(1000);
            }
        }

        public void InitReader()
        {
            if (_readerManager == null) return;
            
            _readerManager.InitDevice();

            if(_readerManager.IsReady)            ReadDeviceInfo();
        }


        private void ReadDeviceInfo()
        {
            var info = _readerManager.ReadDevice();

            Console.WriteLine("Device serial no:{0}", info.SerialNo);
            Console.WriteLine("Device name:{0}", info.DeviceName);
            Console.WriteLine("Device sdk:{0}", info.SDKVersion);
        }

        public void Scan()
        {
            var result = _readerManager.Scan();

            Console.WriteLine("[{0}]:", DateTime.Now);
            Console.WriteLine(result);
        }
    }
}
