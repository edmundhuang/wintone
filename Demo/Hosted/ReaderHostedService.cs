using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using WintoneLib.Core.CardReader;

namespace Demo.Hosted
{
    public class ReaderHostedService : IHostedService
    {
        private readonly IReaderService _readerService;
        private readonly ILogger<ReaderHostedService> _logger;
      
        public ReaderHostedService(IReaderService readerService, ILogger<ReaderHostedService> logger)
        {
            _readerService = readerService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _readerService.ReaderEventHandler += _readerService_ReaderEventHandler;

            return _readerService.MonitorAsync();
        }

        private void _readerService_ReaderEventHandler(object sender, ReaderEventArgs e)
        {
            WriteLog($"证件类型：{e.PassportName}");
            WriteLog(e.Content);

            Console.WriteLine();
            //WriteLog(e.DigitalConent);
        }

        private void WriteLog(string msg)
        {
            if (string.IsNullOrEmpty(msg)) return;

            _logger.LogInformation(msg);
        }

        private void WriteLog(NameValueCollection collection)
        {
            if (collection == null) return;

            foreach (var key in collection.AllKeys)
            {
                var result = $"{key}:{collection[key]}";

                //Console.WriteLine(result);
                _logger.LogInformation(result);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _readerService.Dispose();
            return Task.CompletedTask;
        }
    }
}
