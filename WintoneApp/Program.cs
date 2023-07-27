using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;
using System;
using System.IO;
using WintoneApp.Core.Wintone;
using WintoneApp.ViewModels;
using WintoneApp.Views;

namespace WintoneApp
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
                    services.AddSingleton<MainWindow>();
                    
                    RegisterServices(services);
                })
                .UseSerilog()
                .Build();


            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var loggerFactory = host.Services.GetService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(args.ExceptionObject as Exception, "An error happened");
            };

            Ioc.Default.ConfigureServices(host.Services);

            return host;
        }

        private static void RegisterServices(IServiceCollection services)
        {
            //ui 
            services.AddTransient<DemoView>();
            services.AddTransient<DemoViewModel>();

            services.AddTransient<PassportView>();
            services.AddTransient<PassportViewModel>();

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

        //public static IServiceProvider Services { get; private set; }
    }
}
