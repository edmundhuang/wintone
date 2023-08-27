using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WintoneLib.Core.CardReader
{
    public static class IOC
    {
        public static void AddReaderService(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WintoneOptions>(configuration.GetSection(WintoneOptions.Key));

            services.AddSingleton<IReaderService, ReaderService>();
            services.AddSingleton<ICardReader, CardReader>();
        }
    }
}
