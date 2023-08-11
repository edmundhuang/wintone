using System.Collections.Specialized;

namespace WintoneLib.Core.CardReader
{
    public class CardReader : WintoneReader, ICardReader
    {
        public NameValueCollection Content => throw new System.NotImplementedException();

        public NameValueCollection DigitalContent => throw new System.NotImplementedException();


        private CardType m_CardType = CardType.None;
        public CardType CardType
        {
            get { return m_CardType; }
        }

        public bool Scan()
        {
            throw new System.NotImplementedException();
        }
    }
}
