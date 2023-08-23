using System;
using System.Collections.Specialized;

namespace WintoneLib.Core.CardReader
{
    public class FakeCardReader: CardReader
    {
        private string FakeImagePath = "";

        public override void Init(IReaderOption readerOption)       {        }

        public override int Scan(string saveImageFileName = null, int dg = 6150, int imageType = 3, bool vz = true)
        {
            CardType = CardType.WithId;

            SaveImage(saveImageFileName);

            return 1;
        }

        private void SaveImage(string saveImageFileName)
        {
            throw new NotImplementedException();
        }

        public override NameValueCollection Content
        {
            get => FakeContent();
        }

        public override NameValueCollection DigitalContent
        {
            get => FakeDigitalContent();
        }

        private NameValueCollection FakeDigitalContent()
        {
            var result = new NameValueCollection
            {
                { "姓名", "黄长春" }
            };

            return result;
        }

        private NameValueCollection FakeContent()
        {
            var result = new NameValueCollection
            {
                { "姓名", "黄长春" }
            };

            return result;
        }
    }
}
