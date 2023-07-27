using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WintoneApp.Core.Helpers;

namespace WintoneApp.Core.Wintone
{
    public class CardDevice
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

        public bool InitDevice(string fileName, string configFileName, string userId)
        {
            if (!LoadKernel(fileName)) return false;

            int nRet;
            int nConFig;

            nRet = pInitIDCard(userId, 1, fileName);

            switch (nRet)
            {
                case 1:
                    LogInfo("UserID error.\n");
                    break;
                case 2:
                    LogInfo("Device initialization failed.\n");
                    break;
                case 3:
                    LogInfo("Failed to initialize the certificate core.\n");
                    break;
                case 4:
                    LogInfo("The authorization file was not found.\n");
                    break;
                case 5:
                    LogInfo("Failed to load template file.\n");
                    break;
                case 6:
                    LogInfo("Failed to initialize card reader.\n");
                    break;
                default:
                    break;
            }

            nConFig = pSetConfigByFile(configFileName);


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


        private bool IsDeviceOnline
        {
            get => pCheckDeviceOnline();
        }


        public bool IsReady { get => pInitIDCard != null; }
    }
}
