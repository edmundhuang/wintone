using System.IO;

namespace WintoneLib.Core.CardReader
{
    public class ReaderOption : IReaderOption
    {
        public const string Key = "Wintone";

        public string UserId { get; set; }
        public string LibraryPath { get; set; }
        public string ConfigFileName { get; set; }
        public string KernelFileName { get; set; }

        public string FullKernelPath { get => Path.Combine(LibraryPath, KernelFileName); }
        public string FullConfigPath { get => Path.Combine(LibraryPath, ConfigFileName); }
    }
}
