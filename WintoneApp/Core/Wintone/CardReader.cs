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
    public class CardReader : IDisposable
    {
        private const string DLL_FILE_NAME = "IDCard.dll";

        #region "SDK"

        delegate int InitIDCard([MarshalAs(UnmanagedType.LPWStr)] string userID, int nType, [MarshalAs(UnmanagedType.LPWStr)] string lpDirectory);
        delegate void FreeIDCard();

        delegate bool CheckDeviceOnline();
        delegate int DetectDocument();

        delegate void ResetIDCardID();
        delegate int AddIDCardID(int nMainID, int[] nSubID, int nSubIdCount);

        delegate void SetRecogDG(int nDG);
        delegate void SetRecogVIZ(bool bRecogVIZ);
        delegate void SetSaveImageType(int nImageType);

        delegate int SetConfigByFile([MarshalAs(UnmanagedType.LPWStr)] String strConfigFile);

        delegate int AutoProcessIDCard(ref int nCardType);

        delegate int GetRecogResultEx(int nAttribute, int nIndex, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, ref int nBufferLen);
        delegate int GetFieldNameEx(int nAttribute, int nIndex, [MarshalAs(UnmanagedType.LPWStr)] String ArrBuffer, ref int nBufferLen);

        #endregion "SDK"

        InitIDCard pInitIDCard;
        FreeIDCard pFreeIDCard;

        CheckDeviceOnline pCheckDeviceOnline;

        DetectDocument pDetectDocument;
        AutoProcessIDCard pAutoProcessIDCard;

        ResetIDCardID pResetIDCardID;
        AddIDCardID pAddIDCardID;

        SetRecogDG pSetRecogDG;
        SetRecogVIZ pSetRecogVIZ;
        SetConfigByFile pSetConfigByFile;
        SetSaveImageType pSetSaveImageType;

        GetRecogResultEx pGetRecogResultEx;
        GetFieldNameEx pGetFieldNameEx;

        private ILogger<CardReader> _logger;
        private readonly WintoneOptions _options;

        public CardReader(IOptions<WintoneOptions> options, ILogger<CardReader> logger)
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
            if (!IsDeviceOnline)
            {
                _logger.LogWarning("Reader is offline. Please check power and cable.");
                return;
            }

            int[] nSubID = new int[] { 0 };
            //nSubID[0] = 0;

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

        private bool IsDeviceOnline
        {
            get => pCheckDeviceOnline();
        }


        public bool IsReady { get => pInitIDCard != null; }

        public bool LoadKernel()
        {
            if (!File.Exists(DLLPath))
            {
                _logger.LogWarning("Dll file {0} not found.", DLLPath);
                return false;
            }

            if (IsReady) return true;

            IntPtr hModule = DLLHelper.LoadLibrary(DLLPath);

            pInitIDCard = (InitIDCard)DLLHelper.LoadFunction<InitIDCard>(hModule, "InitIDCard");
            pFreeIDCard = (FreeIDCard)DLLHelper.LoadFunction<FreeIDCard>(hModule, "FreeIDCard");

            pAutoProcessIDCard = (AutoProcessIDCard)DLLHelper.LoadFunction<AutoProcessIDCard>(hModule, "AutoProcessIDCard");

            pCheckDeviceOnline = (CheckDeviceOnline)DLLHelper.LoadFunction<CheckDeviceOnline>(hModule, "CheckDeviceOnline");
            pDetectDocument = (DetectDocument)DLLHelper.LoadFunction<DetectDocument>(hModule, "DetectDocument");

            pResetIDCardID = (ResetIDCardID)DLLHelper.LoadFunction<ResetIDCardID>(hModule, "ResetIDCardID");
            pAddIDCardID = (AddIDCardID)DLLHelper.LoadFunction<AddIDCardID>(hModule, "AddIDCardID");

            pSetRecogVIZ = (SetRecogVIZ)DLLHelper.LoadFunction<SetRecogVIZ>(hModule, "SetRecogVIZ");
            pSetConfigByFile = (SetConfigByFile)DLLHelper.LoadFunction<SetConfigByFile>(hModule, "SetConfigByFile");
            pSetRecogDG = (SetRecogDG)DLLHelper.LoadFunction<SetRecogDG>(hModule, "SetRecogDG");
            pSetSaveImageType = (SetSaveImageType)DLLHelper.LoadFunction<SetSaveImageType>(hModule, "SetSaveImageType");

            pGetRecogResultEx = (GetRecogResultEx)DLLHelper.LoadFunction<GetRecogResultEx>(hModule, "GetRecogResultEx");

            pGetFieldNameEx = (GetFieldNameEx)DLLHelper.LoadFunction<GetFieldNameEx>(hModule, "GetFieldNameEx");

            return pInitIDCard != null;
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
                   var result= GetContent();

                    _logger.LogInformation("card result: {0}", JsonSerializer.Serialize(result));
                }
            }

            isRunning = false;
        }

        private Dictionary<string,string> GetContent()
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

        public void Dispose()
        {
            if (pFreeIDCard == null) return;

            pFreeIDCard();
        }
    }
}
