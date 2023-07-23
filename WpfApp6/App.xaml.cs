using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace WpfApp6
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly MainWindow _mainWindow;

        public App(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            _mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
