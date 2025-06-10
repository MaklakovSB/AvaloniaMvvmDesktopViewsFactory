using System;
using System.Diagnostics;
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
                var viewsFactory = Program.ServiceProvider.GetRequiredService<IViewsFactory>();
                var mainViewModel = Program.ServiceProvider.GetRequiredService<MainWindowViewModel>();
                desktop.MainWindow = viewsFactory.CreateMainView(mainViewModel);

                desktop.Exit += (_, _) =>
                {
                    Debug.WriteLine("Application exiting.");

                    // TODO: Dispose of the views factory if it implements IDisposable.
                    if (viewsFactory is IDisposable disposableFactory)
                    {
                        Debug.WriteLine("Disposing views factory.");
                        disposableFactory.Dispose();
                    }
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}