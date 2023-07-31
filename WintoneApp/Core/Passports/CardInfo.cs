using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace WintoneApp.Core.Passports
{
    public partial class CardInfo : ObservableObject
    {
        [ObservableProperty]
        private DateTime? scanTime;

        [ObservableProperty]
        private string result;
    }
}
