using CommunityToolkit.Mvvm.DependencyInjection;
using System.Windows;
using WintoneApp.Core.Helpers;
using WintoneApp.ViewModels;
using WintoneApp.Views;

namespace WintoneApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //var view=Ioc.Default.GetRequireView<DemoView, DemoViewModel>();
            var view = Ioc.Default.GetRequireView<PassportView,PassportViewModel>();

            Content = view;
        }
    }
}
