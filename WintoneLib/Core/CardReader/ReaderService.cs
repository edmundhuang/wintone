using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace WintoneLib.Core.CardReader
{
    public class ReaderService : IReaderService
    {
        private readonly ILogger<ReaderService> _logger;
        private readonly ReaderOption _option;

        private ICardReader _reader;

        public ReaderService(
            ICardReader reader,
            ILogger<ReaderService> logger,
            IOptions<ReaderOption> options)
        {
            _logger = logger;
            _option = options.Value;

            _reader = reader;

            _reader.Init(_option);
        }

        public void Scan()
        {
            var response = _reader.Scan();

            if(response<0)
            {

            }

        }

        private void WriteLog(string message)
        {

        }
    }

    public interface IReaderService
    {
    }
}
