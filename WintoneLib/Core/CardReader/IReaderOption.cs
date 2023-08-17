namespace WintoneLib.Core.CardReader
{
    public interface IReaderOption
    {
        string UserId { get; set; }
        string LibraryPath { get; set; }
        string ConfigFileName { get; set; }
        string KernelFileName { get; set; }

        string FullKernelPath { get; }
        string FullConfigPath { get; }
    }
}
