using System;
using Avalonia;
using Avalonia.ReactiveUI;
using AvaloniaMvvmDesktopViewsFactory.Factories;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using AvaloniaMvvmDesktopViewsFactory.Service;
using Microsoft.Extensions.DependencyInjection;
using NET8AvaloniaApplicationSample.ViewModels;
using NET8AvaloniaApplicationSample.Views;

namespace NET8AvaloniaApplicationSample
{
    internal sealed class Program
    {
        // This is the entry point of the application for dependencies.
        public static IServiceProvider ServiceProvider { get; private set; } = default!;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Create a service collection and configure services.
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // Start the Avalonia application.
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();

        // Configures the services for dependency injection.
        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IGuidProvider, GuidProvider>();


            // Register the ViewsFactory with the service provider.
            services.AddSingleton<IViewsFactory>(provider =>
            {
                // Initializing the view factory.
                var guidProvider = provider.GetRequiredService<IGuidProvider>();
                var viewAssembly = typeof(MainWindowView).Assembly;
                var viewModelAssembly = typeof(MainWindowViewModel).Assembly;
                var viewsFactory = new ViewsFactory(guidProvider, viewAssembly, viewModelAssembly);

                // Registering additional assemblies the Views and ViewModels.
                //viewsFactory.RegisterAssemblies(otheViewAssembly, otheViewModelAssembly);

                return viewsFactory;
            });

            services.AddTransient<MainWindowViewModel>();
            services.AddTransient<NonModalViewModel>();
        }
    }
}
