using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Windows.Threading;
using WintoneApp.Core.Wintone;

namespace WintoneApp.Core.Passports
{
    public class ReaderManager : IDisposable
    {
        private const string DLL_FILE_NAME = "IDCard.dll";
        private const string CONFIG_FILE_NAME = "IDCardConfig.ini";
        private string IMAGE_FILE_NAME = "demo.jpg";

        private ILogger<ReaderManager> _logger;
        private readonly WintoneOptions _options;
        private readonly CardDevice _device;

        public ReaderManager(IOptions<WintoneOptions> options, ILogger<ReaderManager> logger)
        {
            _logger = logger;
            _options = options.Value;

            _device = new CardDevice();
            _device.LogFactory = WriteLog;

        }

        private void WriteLog(LogLevel level, string msg, object[] args)
        {
            _logger.Log(level, msg, args); 
        }

        public void InitDevice()
        {
            var path = Path.GetFullPath(_options.LibraryPath);

            _device.InitDevice(path, _options.UserId);
        }

        public bool IsReady
        {
            get => _device.IsReady;
        }

        public DeviceInfo ReadDevice()
        {
            DeviceInfo result = new();

            result.SerialNo = _device.GetSerialNo();
            result.DeviceName = _device.GetDeviceName();
            result.SDKVersion=_device.GetSDKVersion();

            return result;
        }

        

        private DispatcherTimer _timer;
        public void StartWatch()
        {
            if (!_device.IsReady) return;

            if (_timer == null)
            {
                _timer = new();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.Tick += DeviceDocumentMonitor;
            }

            _timer.Start();

            return;
        }

        public string Scan()
        {
            var result= _device.Scan();

            if(result == null) return null;

            var imageFileName = Path.GetFullPath(IMAGE_FILE_NAME);
            _device.SaveImage(imageFileName);

            return result;
        }

        private void DeviceDocumentMonitor(object sender, EventArgs e)
        {
            if (!_device.DocumentChanged()) return;

            Scan();
        }

        private void LogInfo(string msg)
        {
            _logger.LogInformation(msg);
        }

        private bool isRunning;
        public void AutoClassAndRecognize()
        {
            if (!_device.IsReady) return;

            isRunning = true;

            _device.Scan();

            isRunning = false;
        }

        public string DLLPath
        {
            get
            {
                var path = Path.GetFullPath(_options.LibraryPath);
                path = Path.Combine(path, DLL_FILE_NAME);
                return path;
            }
        }

        public string CONFIGPATH
        {
            get
            {
                var path = Path.GetFullPath(_options.LibraryPath);
                path = Path.Combine(path, CONFIG_FILE_NAME);
                return path;
            }
        }

        public void Dispose()
        {
            if (_device == null) return;
            _device.Dispose();
        }
    }

    public class DeviceInfo
    {
        public string SerialNo { get; set; }
        public string DeviceName { get; set; }
        public string SDKVersion { get;  set; }
    }
}
