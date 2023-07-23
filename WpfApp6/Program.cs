using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.IO;
using WpfApp6.ViewModels;
using WpfApp6.Views;

namespace WpfApp6
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var host = CreateDefaultHost(args);

            var app= host.Services.GetRequiredService<App>();

            app.Run();
        }

        private static IHost CreateDefaultHost(string[] args)
        {
            InitSerialLog();
            Log.Information("Starting app.");

            var host = Host.CreateDefaultBuilder(args)
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
            services.AddTransient<MainView>();
            services.AddTransient<MainViewModel>();
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
