using System;
using WintoneLib.Core;

namespace WintoneLib.Passports
{
    public class CardInfo : ObservableObject
    {
        public DateTime? ScanTime { get; set; }
        public string Result { get; set; }
    }
}
