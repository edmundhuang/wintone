using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using WintoneApp.Core.Helpers;

namespace WintoneApp.Core.Wintone
{
    public class CardDevice : IDisposable
    {
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

        //SID
        delegate int SDT_OpenPort(int iPort);
        delegate int SDT_ClosePort(int iPort);

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

        SDT_OpenPort pSDT_OpenPort;
        SDT_ClosePort pSDT_ClosePort;

        public Action<LogLevel, string, object[]> LogFactory;

        private string LibPath;

        private const string IDCard_File_Name = "IDCard.dll";
        private const string Config_File_Name = "IDCardConfig.ini";

        public bool InitDevice(string libPath, string userId)
        {
            LibPath = libPath;

            if (!LoadKernel(Path.Combine(libPath, IDCard_File_Name))) return false;

            int nRet;

            nRet = pInitIDCard(userId, 1, libPath);
            switch (nRet)
            {
                case 1:
                    WriteLog("UserID error.\n");
                    break;
                case 2:
                    WriteLog("Device initialization failed.\n");
                    break;
                case 3:
                    WriteLog("Failed to initialize the certificate core.\n");
                    break;
                case 4:
                    WriteLog("The authorization file was not found.\n");
                    break;
                case 5:
                    WriteLog("Failed to load template file.\n");
                    break;
                case 6:
                    WriteLog("Failed to initialize card reader.\n");
                    break;
                default:
                    break;
            }

            int nConFig = pSetConfigByFile(Path.Combine(LibPath, Config_File_Name));

            LoadDLL();

            return true;
        }

        int m_nOpenPort = 0;                // sid port
        private void LoadDLL()
        {
            if (!LoadDllApi()) return;

            for (int iPort = 1001; iPort < 1017; ++iPort)
            {
                if (pSDT_OpenPort(iPort) == 0x90)
                {
                    m_nOpenPort = iPort;
                    break;
                }
            }
        }

        private bool LoadDllApi()
        {
            var DllPath = Path.Combine(LibPath, "sdtapi.dll");

            IntPtr hModuleadi = DLLHelper.LoadLibrary(DllPath);

            if (hModuleadi.ToInt64() == 0)
            {
                WriteLog("Load sdtapi.dll failed \n");
                return false;
            }

            pSDT_OpenPort = (SDT_OpenPort)DLLHelper.LoadFunction<SDT_OpenPort>(hModuleadi, "SDT_OpenPort");
            pSDT_ClosePort = (SDT_ClosePort)DLLHelper.LoadFunction<SDT_ClosePort>(hModuleadi, "SDT_ClosePort");

            return true;
        }

        public bool LoadKernel(string fileName)
        {
            if (IsReady) return true;

            IntPtr hModule = DLLHelper.LoadLibrary(fileName);

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

        public void Scan()
        {
            if (!IsDeviceOnline)
            {

                WriteLog("Reader is offline. Please check power and cable.", null, LogLevel.Warning);
                return;
            }

            int nCardType = 13;

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
                WriteLog("card result: {0}", JsonSerializer.Serialize(result));
            }
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
            }

            return result;
        }


        public bool IsDeviceOnline
        {
            get => pCheckDeviceOnline();
        }

        public bool IsReady { get => pInitIDCard != null; }


        private void WriteLog(string message, params object[] args)
        {
            WriteLog(LogLevel.Information, message, args);
        }

        private void WriteLog(LogLevel level, string message, params object[] args)
        {
            if (LogFactory == null) return;

            LogFactory(level, message, args);
        }

        public void Dispose()
        {
            if (pFreeIDCard == null) return;

            pFreeIDCard();
        }
    }
}
