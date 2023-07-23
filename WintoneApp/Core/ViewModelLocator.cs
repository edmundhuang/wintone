using CommunityToolkit.Mvvm.DependencyInjection;
using WintoneApp.ViewModels;

namespace WintoneApp.Core
{
    public class ViewModelLocator
    {
        public DemoViewModel DemoViewModel
            => Ioc.Default.GetRequiredService<DemoViewModel>();

        //public T ViewModel<T>()
        //    => Ioc.Default.GetService<T>();

        //public MainViewModel MainViewModel
        //    => Program.Services.GetRequiredService<MainViewModel>();
    }
}
