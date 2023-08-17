using System.Collections.Specialized;

namespace WintoneLib.Core.CardReader
{
    public interface ICardReader
    {
        void Init(IReaderOption readerOption);
        int Scan(string saveImageFileName = null, int dg = 6150, int imageType = 3, bool vz = true);
        NameValueCollection Content { get; }
        NameValueCollection DigitalContent { get; }

        CardType CardType { get; }
    }
}
