using System.Windows.Controls;

namespace WintoneApp.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class DemoView : UserControl
    {
        public DemoView()
        {
            InitializeComponent();

            //DataContext = Program.Services!.GetRequiredService<MainViewModel>();
        }
    }
}
