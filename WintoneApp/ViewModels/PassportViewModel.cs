using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WintoneApp.Core.Wintone;

namespace WintoneApp.ViewModels
{
    public partial class PassportViewModel : ObservableObject
    {
        private readonly CardReader _cardReader;

        public PassportViewModel() { }
        public PassportViewModel(CardReader cardReader)
        {
            _cardReader = cardReader;
        }

        [RelayCommand]
        public void InitReader()
        {
            if (_cardReader == null) return;

            _cardReader.LoadKernel();
        }

        [RelayCommand]
        public void StartWatch()
        {
            _cardReader.StartWatch();
        }

        [RelayCommand]
        public void Scan()
        {
            _cardReader.Scan();
        }
    }
}
