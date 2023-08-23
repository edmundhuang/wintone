﻿using System.Collections.Specialized;

namespace WintoneLib.Core.CardReader
{
    public class CardReader : WintoneReader, ICardReader
    {
        private IReaderOption _readerOption;

        public virtual void Init(IReaderOption readerOption)
        {
            _readerOption = readerOption;

            var result = LoadKernel(_readerOption.FullKernelPath);

            if (!result) return;

            var userResult = UserValidate(_readerOption.UserId, _readerOption.LibraryPath);

            if (!userResult) return;

            SetConfigFile(_readerOption.FullConfigPath);
        }

        public virtual int Scan(string saveImageFileName = null, int dg = 6150, int imageType = 3, bool vz = true)
        {
            if (!IsReady) return -100;

            var result = ScanDocument(dg, imageType, vz);

            if (result == null) return -100;

            CardType = (CardType)result.CardType;

            if (result.ScanResult > 0 && !string.IsNullOrEmpty(saveImageFileName))
                SaveImage(saveImageFileName, imageType);

            return result.ScanResult;
        }

        public virtual bool SupportDigital { get => CardType == CardType.WithId || CardType== CardType.BarcodeWithId; }

        public virtual NameValueCollection Content { get => GetContent(); }

        public virtual NameValueCollection DigitalContent { get => GetDigitalContent(); }

        public CardType CardType { get; set; }
    }
}
