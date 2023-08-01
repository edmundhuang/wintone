using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WintoneLib.Core;
using WintoneLib.Passports;

namespace WintoneConsole
{
    public class Program
    {
        public static IConfiguration Configuration { get; private set; }

        [STAThread]
        public static void Main(string[] args)
        {
            using (var host = CreateDefaultHost(args))
            {
                host.StartAsync();

                var app = host.Services.GetRequiredService<App>();
                app.Run();

                host.StopAsync();
            }
        }

        private static IHost CreateDefaultHost(string[] args)
        {
            InitSerialLog();
            Log.Information("Starting app.");

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(configBuilder =>
                {
                    Configuration = configBuilder.Build();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<App>();
                    RegisterServices(services);
                })
                .UseSerilog()
                .Build();

            return host;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            //custom service
            services.AddSingleton<ReaderManager>();


            //configuration 
            services.Configure<WintoneOptions>(Configuration.GetSection(WintoneOptions.Key));
        }

        public static void InitSerialLog()
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("serilog.json").Build()).CreateLogger();
        }
    }

    //internal class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        demo();
    //    }

    //    private static void demo()
    //    {

    //    }
    //}
}
