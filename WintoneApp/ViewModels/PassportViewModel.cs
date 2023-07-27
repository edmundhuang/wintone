using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WintoneApp.Core.Wintone;

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
        }

        [RelayCommand]
        public void StartWatch()
        {
            _readerManager.StartWatch();
        }

        [RelayCommand]
        public void Scan()
        {
            _readerManager.Scan();
        }
    }
}
