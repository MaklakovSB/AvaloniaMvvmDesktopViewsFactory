# AvaloniaMvvmDesktopViewsFactory (EN)

**A library for building desktop applications on the .NET platform using the AvaloniaUI framework, a centralized view factory, and the MVVM architectural pattern.**

---

## Description

`AvaloniaMvvmDesktopViewsFactory` is an infrastructure library for .NET desktop applications that use AvaloniaUI and follow the MVVM architecture. It provides a centralized mechanism for creating, displaying, and closing *window-views* (`Window`) based on their corresponding *view models* (`ViewModel`), eliminating tight coupling between layers.

The library adheres strictly to the MVVM pattern and is structured as a dependency-injected service (DI). To use it, you must configure your DI container and add the `Microsoft.Extensions.DependencyInjection` package.

View–ViewModel association is performed via the `[ViewFor]` attribute or naming convention (`MainViewModel` ↔ `MainView`).

---

## Features

- Creating the application's main window (`MainWindow`) from a given view model.
- Displaying modal and non-modal windows without explicit type references.
- Dialog support with return values (`ShowDialogViewWithResultAsync`).
- Safe window closing based on the associated view model’s unique identifier (`UID`).
- Automatic disposal of view models that implement `IDisposable` when the window closes.
- Support for multiple windows of the same type displayed simultaneously — each linked to a unique view model via its `UID`.
- Cached `ViewModel → View` resolution for better performance.

---

## Requirements

- .NET 6.0 or .NET 8.0  
- Avalonia UI 11.3.0  
- Microsoft.Extensions.DependencyInjection  
- MVVM architecture with DI container support

---

## License  
MIT License

---

## 📦 Usage

1. Create a desktop application targeting .NET 6.0 or .NET 8.0 using the template:

   **Avalonia .NET MVVM App (AvaloniaUI)**

2. Install NuGet packages:
   ```bash
   dotnet add package Microsoft.Extensions.DependencyInjection
   dotnet add package AvaloniaMvvmDesktopViewsFactory
   ```

3. Modify the Program class as follows:
    ```csharp
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
                var guidProvider = provider.GetRequiredService<IGuidProvider>();

                // Use the assembly of the MainWindowView as the view assembly.
                var viewAssembly = typeof(MainWindowView).Assembly;
                return new ViewsFactory(guidProvider, viewAssembly);
            });

            // Register your ViewModels here.
            services.AddTransient<MainWindowViewModel>();
        }
    }
    ```

4. Modify the App class as follows:
    ```csharp
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

                    // Dispose of the views factory if it implements IDisposable.
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
    ```

5. Modify the ViewModelBase class as follows:
    ```csharp
    public class ViewModelBase : ReactiveObject, IUnique
    {
        public Guid Uid
        {
            get => _uid;
            set
            {
                if (_uid != Guid.Empty)
                    throw new InvalidOperationException($"[{nameof(ViewModelBase)}] Uid is already assigned and cannot be changed.");

                if (value == Guid.Empty)
                    throw new ArgumentException($"[{nameof(ViewModelBase)}] Uid must not be empty.", nameof(value));

                this.RaiseAndSetIfChanged(ref _uid, value);
            }
        }
        public Guid _uid;
    }
    ```

6. Minimal working pattern for MainWindowViewModel:
    ```csharp
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public MainWindowViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            _disposables.Dispose();

            _isDisposed = true;

            Debug.WriteLine($"[{nameof(MainWindowViewModel)}] The Dispose method is complete for {nameof(MainWindowViewModel)}, Guid {Uid}.");
        }
    }
    ```

Now you no longer need to hold a direct reference to a window instance in order to open or close a view — this can be done via the factory, for example:
    ```csharp
    var questionBoxViewModel = new QuestionBoxViewModel("Are you sure ...?", "Question");
    var result = await _viewsService.ShowDialogViewWithResultAsync<QuestionBoxViewModel, QuestionBoxResult>(questionBoxViewModel);

    if (result == QuestionBoxResult.Ok)
    {
    }
    else
    {
    }
    ```

    You can also explore the code of the demo application or reach out to the author with a question.

---

# AvaloniaMvvmDesktopViewsFactory (RU)

** Библиотека для построения десктопных приложений на платформе .NET с использованием фреймворка AvaloniaUI, централизованной фабрики представлений и архитектурного паттерна MVVM.**

---

## Описание

`AvaloniaMvvmDesktopViewsFactory` — это инфраструктурная библиотека для .NET-приложений с графическим интерфейсом, использующих AvaloniaUI и архитектуру MVVM. Она предоставляет централизованный механизм создания, отображения и закрытия *окон-представлений* (`Window`), ассоциированных с соответствующими *моделями представления* (`ViewModel`), устраняя жёсткие зависимости между слоями приложения.

Библиотека ориентирована на строгое соблюдение паттерна MVVM и организована в виде сервиса, интегрируемого через механизм внедрения зависимостей (DI). Для использования требуется предварительная настройка контейнера и установка пакета `Microsoft.Extensions.DependencyInjection`.

Сопоставление представлений с моделями осуществляется с помощью атрибута `[ViewFor]` либо на основе соглашения об именовании (`MainViewModel` ↔ `MainView`).

---

## Возможности

- Создание основного окна приложения (`MainWindow`) по заданной модели представления.
- Отображение немодальных и модальных окон без необходимости прямого указания типа представления.
- Поддержка диалоговых окон с возвращаемым результатом (`ShowDialogViewWithResultAsync`).
- Безопасное закрытие окна, связанного с конкретной моделью представления, с учётом уникального идентификатора (`UID`).
- Автоматическое освобождение ресурсов моделей, реализующих `IDisposable`, при закрытии окна.
- Поддержка одновременного отображения нескольких окон одного типа: каждое окно связано со своей моделью и управляется независимо благодаря системе `UID`.
- Кэширование сопоставлений `ViewModel → View` для повышения производительности.

---

## Требования

- .NET 6.0 или .NET 8.0  
- Avalonia UI 11.3.0  
- Microsoft.Extensions.DependencyInjection  
- MVVM-архитектура с поддержкой внедрения зависимостей (DI)

---

## Лицензия  
MIT License

---

## 📦 Использование

1. Создайте десктопное приложение под .NET 6.0 или .NET 8.0, используя шаблон:

   **Avalonia .NET MVVM App (AvaloniaUI)**

2. Установите NuGet-пакеты:
   ```bash
   dotnet add package Microsoft.Extensions.DependencyInjection
   dotnet add package AvaloniaMvvmDesktopViewsFactory
   ```

3. Измените класс Program следующим образом:
    ```csharp
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
                var guidProvider = provider.GetRequiredService<IGuidProvider>();

                // Use the assembly of the MainWindowView as the view assembly.
                var viewAssembly = typeof(MainWindowView).Assembly;
                return new ViewsFactory(guidProvider, viewAssembly);
            });

            // Register your ViewModels here.
            services.AddTransient<MainWindowViewModel>();
        }
    }
    ```

4. Измените класс App следующим образом:
    ```csharp
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

                    // Dispose of the views factory if it implements IDisposable.
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
    ```

5. Измените класс ViewModelBase следующим образом:
    ```csharp
    public class ViewModelBase : ReactiveObject, IUnique
    {
        public Guid Uid
        {
            get => _uid;
            set
            {
                if (_uid != Guid.Empty)
                    throw new InvalidOperationException($"[{nameof(ViewModelBase)}] Uid is already assigned and cannot be changed.");

                if (value == Guid.Empty)
                    throw new ArgumentException($"[{nameof(ViewModelBase)}] Uid must not be empty.", nameof(value));

                this.RaiseAndSetIfChanged(ref _uid, value);
            }
        }
        public Guid _uid;
    }
    ```

6. Минимальный шаблон реализации MainWindowViewModel:
    ```csharp
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public MainWindowViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            _disposables.Dispose();

            _isDisposed = true;

            Debug.WriteLine($"[{nameof(MainWindowViewModel)}] The Dispose method is complete for {nameof(MainWindowViewModel)}, Guid {Uid}.");
        }
    }
    ```

Теперь для открытия и закрытия представлений вам не нужно иметь ссылку экземпляр окна, вы можете это делать через обращения к фабрике подобным образом:
    ```csharp
    var questionBoxViewModel = new QuestionBoxViewModel("Вы уверены ... ?", "Вопрос.");
    var result = await _viewsService.ShowDialogViewWithResultAsync<QuestionBoxViewModel, QuestionBoxResult>(questionBoxViewModel);

    if (result == QuestionBoxResult.Ok)
    {
    }
    else
    {
    }
    ```

    Вы также можете изучить код демонстрационного приложения или обратиться к автору с вопросом.

    ---