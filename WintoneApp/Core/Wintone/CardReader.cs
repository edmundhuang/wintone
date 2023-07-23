using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Runtime.InteropServices;
using WintoneApp.Core.Helpers;

namespace WintoneApp.Core.Wintone
{
    public class CardReader : IDisposable
    {
        private const string DLL_FILE_NAME = "IDCard.dll";

        #region "SDK"

        delegate int InitIDCard([MarshalAs(UnmanagedType.LPWStr)] string userID, int nType, [MarshalAs(UnmanagedType.LPWStr)] string lpDirectory);

        #endregion "SDK"

        InitIDCard pInitIDCard;

        private ILogger<CardReader> _logger;
        private readonly WintoneOptions _options;


        public CardReader(IOptions<WintoneOptions> options, ILogger<CardReader> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public bool InitReader()
        {


            return true;
        }

        private void LoadKernel()
        {
            try
            {
                IntPtr hModule = DLLHelper.LoadLibrary(DLLPath);
                pInitIDCard = (InitIDCard)DLLHelper.LoadFunction<InitIDCard>(hModule, "InitIDCard");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LoadKernel error.");
            }
        }


        public string DLLPath { get => Path.Combine(_options.LibraryPath, DLL_FILE_NAME); }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
