using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace WintoneApp.ViewModels
{
    public partial class DemoViewModel : ObservableObject
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

        public DemoViewModel()
        {
                
        }
        //public string Title { get; set; } = "Main";
    }
}
