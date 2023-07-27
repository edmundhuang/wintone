using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Threading;
using WintoneApp.Core.Helpers;

namespace WintoneApp.Core.Wintone
{
    public class ReaderManager : IDisposable
    {
        private const string DLL_FILE_NAME = "IDCard.dll";
        private const string CONFIG_FILE_NAME = "IDCardConfig.ini";

        private ILogger<ReaderManager> _logger;
        private readonly WintoneOptions _options;
        private readonly CardDevice _device;

        public ReaderManager(IOptions<WintoneOptions> options, ILogger<ReaderManager> logger)
        {
            _logger = logger;
            _options = options.Value;

            
        }

        private DispatcherTimer _timer;
        public void StartWatch()
        {
            if (!IsReady) return;

            if (_timer == null)
            {
                _timer = new();
                _timer.Interval = TimeSpan.FromMilliseconds(100);
                _timer.Tick += DeviceDocumentMonitor;
            }

            _timer.Start();

            return;
        }

        public void Scan()
        {
            _device.Scan();

            if (!IsDeviceOnline)
            {
                _logger.LogWarning("Reader is offline. Please check power and cable.");
                return;
            }

            //int[] nSubID = new int[] { 0 };
            ////nSubID[0] = 0;

            int[] nSubID = new int[1];
            nSubID[0] = 0;

            pResetIDCardID();
            int nRet = pAddIDCardID(nCardType, nSubID, 1);

            //get param
            int nDG = 0;
            int nSaveImage = 1;

            bool bVIZ = true;

            int cardType = 0;

            pSetRecogDG(nDG);
            pSetSaveImageType(nSaveImage);
            pSetRecogVIZ(bVIZ);

            nRet = pAutoProcessIDCard(ref cardType);

            if (nRet > 0)
            {
                var result = GetContent();
                _logger.LogInformation("card result: {0}", JsonSerializer.Serialize(result));
            }
        }

        private int nCardType = 13;
        private void DeviceDocumentMonitor(object sender, EventArgs e)
        {
            Scan();
        }

        //public bool LoadKernel(string fileName)
        //{
        //    if (!File.Exists(fileName))
        //    {
        //        _logger.LogWarning("Dll file {0} not found.", DLLPath);
        //        return false;
        //    }

        //    if (IsReady) return true;

        //    IntPtr hModule = DLLHelper.LoadLibrary(DLLPath);

        //    pInitIDCard = (InitIDCard)DLLHelper.LoadFunction<InitIDCard>(hModule, "InitIDCard");
        //    pFreeIDCard = (FreeIDCard)DLLHelper.LoadFunction<FreeIDCard>(hModule, "FreeIDCard");

        //    pAutoProcessIDCard = (AutoProcessIDCard)DLLHelper.LoadFunction<AutoProcessIDCard>(hModule, "AutoProcessIDCard");

        //    pCheckDeviceOnline = (CheckDeviceOnline)DLLHelper.LoadFunction<CheckDeviceOnline>(hModule, "CheckDeviceOnline");
        //    pDetectDocument = (DetectDocument)DLLHelper.LoadFunction<DetectDocument>(hModule, "DetectDocument");

        //    pResetIDCardID = (ResetIDCardID)DLLHelper.LoadFunction<ResetIDCardID>(hModule, "ResetIDCardID");
        //    pAddIDCardID = (AddIDCardID)DLLHelper.LoadFunction<AddIDCardID>(hModule, "AddIDCardID");

        //    pSetRecogVIZ = (SetRecogVIZ)DLLHelper.LoadFunction<SetRecogVIZ>(hModule, "SetRecogVIZ");
        //    pSetConfigByFile = (SetConfigByFile)DLLHelper.LoadFunction<SetConfigByFile>(hModule, "SetConfigByFile");
        //    pSetRecogDG = (SetRecogDG)DLLHelper.LoadFunction<SetRecogDG>(hModule, "SetRecogDG");
        //    pSetSaveImageType = (SetSaveImageType)DLLHelper.LoadFunction<SetSaveImageType>(hModule, "SetSaveImageType");

        //    pGetRecogResultEx = (GetRecogResultEx)DLLHelper.LoadFunction<GetRecogResultEx>(hModule, "GetRecogResultEx");

        //    pGetFieldNameEx = (GetFieldNameEx)DLLHelper.LoadFunction<GetFieldNameEx>(hModule, "GetFieldNameEx");

        //    return pInitIDCard != null;
        //}

        //public bool InitDevice()
        //{
        //    string UserID = _options.UserId;

        //    int nRet;
        //    int nConFig;

        //    nRet = pInitIDCard(UserID, 1, DLLPath);

        //    switch (nRet)
        //    {
        //        case 1:
        //            LogInfo("UserID error.\n");
        //            break;
        //        case 2:
        //            LogInfo("Device initialization failed.\n");
        //            break;
        //        case 3:
        //            LogInfo("Failed to initialize the certificate core.\n");
        //            break;
        //        case 4:
        //            LogInfo("The authorization file was not found.\n");
        //            break;
        //        case 5:
        //            LogInfo("Failed to load template file.\n");
        //            break;
        //        case 6:
        //            LogInfo("Failed to initialize card reader.\n");
        //            break;
        //        default:
        //            break;
        //    }

        //    nConFig = pSetConfigByFile(CONFIGPATH);

        //    return true;
        //}



        private void LogInfo(string msg)
        {
            _logger.LogInformation(msg);
        }

        private bool isRunning;
        public void AutoClassAndRecognize()
        {
            if (!IsReady) return;

            if (isRunning) return;

            isRunning = true;

            var nRet = pDetectDocument();

            if (nRet == 1)
            {
                int nCardType = 0;
                nRet = pAutoProcessIDCard(ref nCardType);

                if (nRet > 0)
                {
                    var result = GetContent();

                    _logger.LogInformation("card result: {0}", JsonSerializer.Serialize(result));
                }
            }

            isRunning = false;
        }

        private Dictionary<string, string> GetContent()
        {
            Dictionary<string, string> result = new();

            int MAX_CH_NUM = 128;
            int nBufLen = MAX_CH_NUM * sizeof(byte);

            for (int i = 0; ; i++)
            {
                string cArrFieldValue = new string('\0', MAX_CH_NUM);
                string cArrFieldName = new('\0', MAX_CH_NUM);

                int nRet = pGetRecogResultEx(1, i, cArrFieldValue, ref nBufLen);
                if (nRet == 3)
                    break;

                nBufLen = MAX_CH_NUM * sizeof(byte);
                pGetFieldNameEx(1, i, cArrFieldName, ref nBufLen);

                if (string.IsNullOrEmpty(cArrFieldName)) continue;

                result.TryAdd(cArrFieldName, cArrFieldValue);
                //nBufLen = MAX_CH_NUM * sizeof(byte);
            }

            return result;
        }

        public string DLLPath
        {
            get
            {
                var path = Path.GetFullPath(_options.LibraryPath);
                path = Path.Combine(path, DLL_FILE_NAME);
                return path;
            }
        }

        public string CONFIGPATH
        {
            get
            {
                var path = Path.GetFullPath(_options.LibraryPath);
                path = Path.Combine(path, CONFIG_FILE_NAME);
                return path;
            }
        }

        public void Dispose()
        {
            if (pFreeIDCard == null) return;

            pFreeIDCard();
        }
    }
}
