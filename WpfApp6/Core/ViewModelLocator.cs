using CommunityToolkit.Mvvm.DependencyInjection;
using WpfApp6.ViewModels;

namespace WpfApp6.Core
{
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel
            => Ioc.Default.GetRequiredService<MainViewModel>();

        //public T ViewModel<T>()
        //    => Ioc.Default.GetService<T>();

        //public MainViewModel MainViewModel
        //    => Program.Services.GetRequiredService<MainViewModel>();
    }
}
