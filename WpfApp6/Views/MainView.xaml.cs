using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using WpfApp6.ViewModels;

namespace WpfApp6.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();

            //DataContext = Program.Services!.GetRequiredService<MainViewModel>();
        }
    }
}
