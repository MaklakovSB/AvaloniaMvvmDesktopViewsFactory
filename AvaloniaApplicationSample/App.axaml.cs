using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaApplicationSample.ViewModels;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplicationSample
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var viewsService = Program.ServiceProvider.GetRequiredService<IViewsFactory>();
                var mainViewModel = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = viewsService.CreateMainView(mainViewModel);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}