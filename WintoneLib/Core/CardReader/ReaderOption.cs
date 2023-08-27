using System.IO;

namespace WintoneLib.Core.CardReader
{
    public class WintoneOptions : IReaderOption
    {
        public const string Key = "Wintone";

        public string UserId { get; set; } = "73806677262145096873";
        public string LibraryPath { get; set; }
        public string ConfigFileName { get; set; } = "IDCardConfig.ini";
        public string KernelFileName { get; set; } = "IDCard.dll";

        public string FullKernelPath { get => Path.Combine(LibraryPath, KernelFileName); }
        public string FullConfigPath { get => Path.Combine(LibraryPath, ConfigFileName); }
        public int Interval { get; set; } = 1000;
    }
}
