using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace WintoneLib.Core.CardReader
{
    public class ReaderService : IReaderService
    {
        private readonly PassportTypeService _passportTypeService = new PassportTypeService();

        public event ReaderEventHandler ReaderEventHandler;

        private readonly ILogger<ReaderService> _logger;
        private readonly WintoneOptions _option;

        private ICardReader _reader;

        public ReaderService(
            ICardReader reader,
            ILogger<ReaderService> logger,
            IOptions<WintoneOptions> options)
        {
            _logger = logger;
            _option = options.Value;

            _reader = reader;

            _reader.Init(_option);
        }

        public Task MonitorAsync()
        {
            return Task.Run(Monitor);
        }

        private void Monitor()
        {
            while (true)
            {
                if (_reader.DocumentChanged) Scan();

                Thread.Sleep(_option.Interval);
            }
        }

        public void Scan()
        {
            var response = _reader.Scan();

            if (response > 0)
            {
                _logger.LogInformation("检测到新证件放入。");

                var passportTypeId = response.ToString();

                var content = _reader.Content;

                var digitalContent = _reader.SupportDigital ? _reader.DigitalContent : null;

                var arg = new ReaderEventArgs { 
                    PassportTypeId= passportTypeId, 
                    PassportName= _passportTypeService.GetName(passportTypeId),
                    Content = content, 
                    DigitalConent = digitalContent };

                ReaderEventHandler?.Invoke(this, arg);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

    }

    public interface IReaderService : IDisposable
    {
        Task MonitorAsync();

        event ReaderEventHandler ReaderEventHandler;
    }

    // Declare the delegate (if using non-generic pattern).
    public delegate void ReaderEventHandler(object sender, ReaderEventArgs e);

    public class ReaderEventArgs : EventArgs
    {
        public string PassportTypeId { get; set; }
        public string PassportName { get; set; }
        public NameValueCollection Content { get; set; }
        public NameValueCollection DigitalConent { get; set; }
    }
}
