using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Text;
using WintoneLib.Core.Helpers;

namespace WintoneLib.Core.CardReader
{
    public class WintoneReader : IDisposable
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

        #region "SDK Funtion"

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

        #endregion "SDK Funtion"

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

        public bool UserValidate(string userId, string libPath)
        {
            int nRet;

            nRet = pInitIDCard(userId, 1, libPath);
            switch (nRet)
            {
                case 1:
                    m_ErrorMessage = "UserID error.\n";
                    return false;
                case 2:
                    m_ErrorMessage = "Device initialization failed.\n";
                    return false;
                case 3:
                    m_ErrorMessage = "Failed to initialize the certificate core.\n";
                    return false;
                case 4:
                    m_ErrorMessage = "The authorization file was not found.\n";
                    return false;
                case 5:
                    m_ErrorMessage = "Failed to load template file.\n";
                    return false;
                case 6:
                    m_ErrorMessage = "Failed to initialize card reader.\n";
                    return false;
                default:
                    break;
            }

            return true;
        }

        public bool DocumentChanged
        {
            get
            {
                int nRet = pDetectDocument();
                return nRet == 1;
            }
        }

        public void SetConfigFile(string fileName)
        {
            int nConFig = pSetConfigByFile(fileName);
        }

        public ScanResponse ScanDocument(int dg, int saveImage = 3, bool viz = true)
        {
            if (!IsReady) return null;

            if (!IsDeviceOnline)
            {
                m_ErrorMessage = "Reader is offline. Please check power and cable.";
                return null;
            }

            int cardType = 0;

            pSetRecogDG(dg);
            pSetSaveImageType(saveImage);
            pSetRecogVIZ(viz);

            int nRet = pAutoProcessIDCard(ref cardType);

            if (nRet > 0) return new ScanResponse(nRet, cardType);

            return null;
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

        public void SaveImage(string fileName, int nType)
        {
            pSaveImageEx(fileName, nType);
        }

        public NameValueCollection GetContent()
        {
            var result = new NameValueCollection();

            for (int i = 0; ; i++)
            {
                var sValue = GetValue(i);
                if (sValue == null) break;

                var sName = GetName(i);
                result.Add(sName, sValue);
            }

            return result;
        }

        public NameValueCollection GetDigitalContent()
        {
            var result = new NameValueCollection();

            for (int i = 0; ; i++)
            {
                var sValue = GetValue(i, 0);
                if (sValue == null) break;

                var sName = GetName(i, 0);
                result.Add(sName, sValue);
            }

            return result;
        }

        private string GetValue(int index, int digitFlag = 1) //digitFlag=1 get OCR content, =0 get digital content.
        {
            int MAX_CH_NUM = 128;
            int nBufLen = MAX_CH_NUM * sizeof(byte);
            var sValue = new string('\0', MAX_CH_NUM);

            int nRet = pGetRecogResultEx(digitFlag, index, sValue, ref nBufLen);

            if (nRet == 0) return sValue.Substring(0, nBufLen);

            return null;
        }

        private string GetName(int index, int digitFlag = 1)
        {
            int MAX_CH_NUM = 128;
            int nBufLen = MAX_CH_NUM * sizeof(byte);
            var sValue = new string('\0', MAX_CH_NUM);

            pGetFieldNameEx(digitFlag, index, sValue, ref nBufLen);

            return sValue.Substring(0, nBufLen);
        }


        int m_nOpenPort = 0;                // sid port
        public void LoadDLL(string fileName)
        {
            if (!LoadDllApi(fileName)) return;

            for (int iPort = 1001; iPort < 1017; ++iPort)
            {
                if (pSDT_OpenPort(iPort) == 0x90)
                {
                    m_nOpenPort = iPort;
                    break;
                }
            }
        }

        private bool LoadDllApi(string fileName)
        {
            //var DllPath = Path.Combine(LibPath, "sdtapi.dll");

            IntPtr hModuleadi = DLLHelper.LoadLibrary(fileName);

            if (hModuleadi.ToInt64() == 0)
            {
                m_ErrorMessage = "Load sdtapi.dll failed \n";
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

        private string m_ErrorMessage;
        public string ErrorMessage { get => m_ErrorMessage; }
        public bool IsDeviceOnline { get => pCheckDeviceOnline != null && pCheckDeviceOnline(); }
        public bool IsReady { get => pInitIDCard != null; }

        public void Dispose()
        {
            if (pFreeIDCard == null) return;

            pFreeIDCard();
        }
    }

    public class ScanResponse
    {
        public ScanResponse() { }
        public ScanResponse(int result, int cardType)
        {
            ScanResult = result;
            CardType = cardType;
        }

        public int ScanResult { get; set; }
        public int CardType { get; set; }
    }
}
