using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace WpfApp6.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title="Hello world";

        [RelayCommand(CanExecute =nameof(CanTest))]
        private void Test()
        {
            throw new ApplicationException("The app crashed");
        }

        public bool CanTest()
        {
            return false;
        }

        public MainViewModel()
        {
                
        }
        //public string Title { get; set; } = "Main";
    }
}
