using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using WintoneApp.Core.Passports;

namespace WintoneApp.ViewModels
{
    public partial class PassportViewModel : ObservableObject
    {
        private readonly ReaderManager _readerManager;

        public PassportViewModel() { }
        public PassportViewModel(ReaderManager readerManager)
        {
            _readerManager = readerManager;
        }

        [RelayCommand]
        public void InitReader()
        {
            if (_readerManager == null) return;

            _readerManager.InitDevice();

            ReadDeviceInfo();
        }

        private void ReadDeviceInfo()
        {
            var result = _readerManager.ReadDevice();

            SerialNo = result.SerialNo;
            DeviceName = result.DeviceName;
            Version = result.SDKVersion;
        }

        public bool CanInitReader()=> _readerManager.IsReady;

        [RelayCommand]
        public void StartWatch()
        {
            _readerManager.StartWatch();
        }

        [RelayCommand]
        public void Scan()
        {
           var result= _readerManager.Scan();

            IdResult = result;
        }

        [ObservableProperty]
        private string serialNo;

        [ObservableProperty]
        private string deviceName;

        [ObservableProperty]
        private string version;

        [ObservableProperty]
        private string idResult;
    }
}
