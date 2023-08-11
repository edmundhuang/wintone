using System.Collections.Specialized;
using System.IO;

namespace WintoneLib.Core.CardReader
{
    public interface ICardReader
    {
        bool Scan();
        NameValueCollection Content { get; }
        NameValueCollection DigitalContent { get; }

        CardType CardType { get; }
    }
}
