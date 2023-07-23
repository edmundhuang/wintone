using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows.Controls;

namespace WintoneApp.Core.Helpers
{
    public static class DIHelper
    {
        public static TView GetRequireView<TView, TViewModel>(this IServiceProvider provider)
            where TView : UserControl
        {
            var view= provider.GetRequiredService<TView>();
            if (view == null) return null;

            view.DataContext= provider.GetRequiredService<TViewModel>();

            return view;
        }

    }
}
