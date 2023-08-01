using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using WintoneLib.Core.Helpers;

namespace WintoneLib.Core
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

        delegate int SetConfigByFile([MarshalAs(UnmanagedType.LPWStr)] string strConfigFile);

        delegate int AutoProcessIDCard(ref int nCardType);

        delegate int GetRecogResultEx(int nAttribute, int nIndex, [MarshalAs(UnmanagedType.LPWStr)] string lpBuffer, ref int nBufferLen);
        delegate int GetFieldNameEx(int nAttribute, int nIndex, [MarshalAs(UnmanagedType.LPWStr)] string ArrBuffer, ref int nBufferLen);

        //SID
        delegate int SDT_OpenPort(int iPort);
        delegate int SDT_ClosePort(int iPort);
        delegate int SDT_StartFindIDCard(int iPort, ref byte pRAPDU, int iIfOpen);
        delegate int SDT_SelectIDCard(int iPort, ref byte pRAPDU, int iIfOpen);
        delegate int SDT_ReadBaseMsg(int iPort, ref byte pucCHMsg, ref int puiCHMsgLen, ref byte pucPHMsg, ref int puiPHMsgLen, int iIfOpen);
        delegate int SDT_ReadNewAppMsg(int iPort, ref byte pucAppMsg, ref int puiAppMsgLen, int iIfOpen);
        delegate int GetBmp(string filename, int nType);
        delegate int SDT_GetSAMIDToStr(int iPortID, ref byte pcSAMIDStr, int iIfOpen);
        delegate int SDT_GetSAMID(int iPortID, ref byte pcSAMID, int iIfOpen);

        delegate int GetDeviceSN([MarshalAs(UnmanagedType.LPWStr)] string ArrSn, int nLength);
        delegate int GetCurrentDevice([MarshalAs(UnmanagedType.LPWStr)] string ArrDeviceName, int nLength);
        delegate void GetVersionInfo([MarshalAs(UnmanagedType.LPWStr)] string ArrVersion, int nLength);

        delegate int SaveImageEx([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, int nType);
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

        SDT_StartFindIDCard pSDT_StartFindIDCard;
        SDT_SelectIDCard pSDT_SelectIDCard;
        SDT_ReadBaseMsg pSDT_ReadBaseMsg;
        SDT_ReadNewAppMsg pSDT_ReadNewAppMsg;
        GetBmp pGetBmp;
        SDT_GetSAMIDToStr pSDT_GetSAMIDToStr;
        SDT_GetSAMID pSDT_GetSAMID;

        GetDeviceSN pGetDeviceSN;
        GetCurrentDevice pGetCurrentDevice;

        GetVersionInfo pGetVersionInfo;

        SaveImageEx pSaveImageEx;

        public Action<LogLevel, string, object[]> LogFactory;

        private string LibPath;

        private const string IDCard_File_Name = "IDCard.dll";
        private const string Config_File_Name = "IDCardConfig.ini";

        public bool InitDevice(string libPath, string userId)
        {
            LibPath = libPath;

            var result = LoadKernel(Path.Combine(libPath, IDCard_File_Name));
            if (!result)
            {
                WriteLog("Load Kernel failed.");
                return false;
            }
                

            //if (!LoadKernel(Path.Combine(libPath, IDCard_File_Name))) return false;

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
            pSDT_StartFindIDCard = (SDT_StartFindIDCard)DLLHelper.LoadFunction<SDT_StartFindIDCard>(hModuleadi, "SDT_StartFindIDCard");
            pSDT_SelectIDCard = (SDT_SelectIDCard)DLLHelper.LoadFunction<SDT_SelectIDCard>(hModuleadi, "SDT_SelectIDCard");
            pSDT_ReadBaseMsg = (SDT_ReadBaseMsg)DLLHelper.LoadFunction<SDT_ReadBaseMsg>(hModuleadi, "SDT_ReadBaseMsg");
            pSDT_ReadNewAppMsg = (SDT_ReadNewAppMsg)DLLHelper.LoadFunction<SDT_ReadNewAppMsg>(hModuleadi, "SDT_ReadNewAppMsg");
            pSDT_GetSAMIDToStr = (SDT_GetSAMIDToStr)DLLHelper.LoadFunction<SDT_GetSAMIDToStr>(hModuleadi, "SDT_GetSAMIDToStr");
            pSDT_GetSAMID = (SDT_GetSAMID)DLLHelper.LoadFunction<SDT_GetSAMID>(hModuleadi, "SDT_GetSAMID");


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

            pGetDeviceSN = (GetDeviceSN)DLLHelper.LoadFunction<GetDeviceSN>(hModule, "GetDeviceSN");
            pGetCurrentDevice = (GetCurrentDevice)DLLHelper.LoadFunction<GetCurrentDevice>(hModule, "GetCurrentDevice");
            pGetVersionInfo = (GetVersionInfo)DLLHelper.LoadFunction<GetVersionInfo>(hModule, "GetVersionInfo");

            pGetRecogResultEx = (GetRecogResultEx)DLLHelper.LoadFunction<GetRecogResultEx>(hModule, "GetRecogResultEx");

            pGetFieldNameEx = (GetFieldNameEx)DLLHelper.LoadFunction<GetFieldNameEx>(hModule, "GetFieldNameEx");

            pSaveImageEx = (SaveImageEx)DLLHelper.LoadFunction<SaveImageEx>(hModule, "SaveImageEx");
            return pInitIDCard != null;
        }

        public bool DocumentChanged()
        {
            int nRet = pDetectDocument();
            return nRet == 1;
        }

        public string Scan()
        {
            if (!IsReady) return null;

            if (!IsDeviceOnline)
            {
                WriteLog("Reader is offline. Please check power and cable.", null, LogLevel.Warning);
                return string.Empty;
            }

            int nDG = 6150;
            int nSaveImage = 3;

            bool bVIZ = true;

            int cardType = 0;

            pSetRecogDG(nDG);
            pSetSaveImageType(nSaveImage);
            pSetRecogVIZ(bVIZ);

            int nRet = pAutoProcessIDCard(ref cardType);

            if (nRet > 0) return GetContent();

            return string.Empty;
        }

        public void SaveImage(string fileName)
        {
            int nRet = pSaveImageEx(fileName, 3);
        }

        private string GetContent()
        {
            var sb = new StringBuilder();

            int MAX_CH_NUM = 128;
            int nBufLen = MAX_CH_NUM * sizeof(byte);

            for (int i = 0; ; i++)
            {
                string cArrFieldValue = new string('\0', MAX_CH_NUM);
                string cArrFieldName = new string('\0', MAX_CH_NUM);
                int nRet = pGetRecogResultEx(1, i, cArrFieldValue, ref nBufLen);
                if (nRet == 3)
                    break;
                nBufLen = MAX_CH_NUM * sizeof(byte);
                pGetFieldNameEx(1, i, cArrFieldName, ref nBufLen);

                sb.Append(cArrFieldName.Trim('\0'));
                sb.AppendLine(cArrFieldValue.Trim('\0'));
            }

            return sb.ToString();
        }


        public void Add2Collection(NameValueCollection collection, string name, string value)
        {
            name = name.Trim('\0');
            value = value.Trim('\0');

            collection.Add(name, value);
        }

        public bool IsDeviceOnline
        {
            get => pCheckDeviceOnline != null && pCheckDeviceOnline();
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

        public string GetSerialNo()
        {
            var serialNo = new string('\0', 16);
            int nRet = pGetDeviceSN(serialNo, 16);

            if (nRet == 0) return serialNo.Trim('\0');

            return null;
        }

        public string GetDeviceName()
        {
            var result = new string('\0', 128);

            pGetCurrentDevice(result, 128);
            return result.Trim('\0');
        }

        public string GetSDKVersion()
        {
            var result = new string('\0', 128);

            pGetVersionInfo(result, 128);
            return result.Trim('\0');
        }
    }
}
