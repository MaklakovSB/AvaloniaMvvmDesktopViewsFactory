using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaMvvmDesktopViewsFactory.Attributes;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;

namespace AvaloniaMvvmDesktopViewsFactory.Factories
{
    public class ViewsFactory : IViewsFactory
    {
        private readonly IGuidProvider _guidProvider;
        private readonly ConcurrentDictionary<Type, Type> _viewTypeCache = new();
        private readonly Func<Type, Type?> _viewTypeResolver;

        public ViewsFactory(IGuidProvider guidProvider, Func<Type, Type?>? viewTypeResolver = null)
        {
            _guidProvider = guidProvider;
            _viewTypeResolver = viewTypeResolver ?? DefaultViewTypeResolver;
        }

        public Window CreateMainView<TViewModel>(TViewModel mainViewModel)
            where TViewModel : class, IUnique
        {
            EnsureViewModelHasUid(mainViewModel);
            var window = CreateView(mainViewModel, WindowStartupLocation.Manual);
            SetupDisposableHandling(mainViewModel, window);
            return window;
        }

        public async Task ShowNonModalWindowAsync<TViewModel>(
            TViewModel newViewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterScreen)
            where TViewModel : class, IUnique
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EnsureViewModelHasUid(newViewModel);
                var window = CreateView(newViewModel, location);
                SetupDisposableHandling(newViewModel, window);
                window.Show();
            });
        }

        public async Task ShowModalViewAsync<TViewModel>(
            TViewModel newViewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner,
            TViewModel? ownerViewModel = null)
            where TViewModel : class, IUnique
        {
            var (view, ownerView) = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EnsureViewModelHasUid(newViewModel);
                var v = CreateView(newViewModel, location);
                var owner = GetOwnerWindow(ownerViewModel);
                SetupDisposableHandling(newViewModel, v);
                return (v, owner);
            });

            await Dispatcher.UIThread.InvokeAsync(() => view.ShowDialog(ownerView));
        }

        public async Task<TResult> ShowDialogViewWithResultAsync<TViewModel, TResult>(
            TViewModel newViewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner,
            TViewModel? ownerViewModel = null)
            where TViewModel : class, ICloseable<TResult>, IUnique
        {
            var (view, ownerView) = await Dispatcher.UIThread.InvokeAsync(() =>
            {
                EnsureViewModelHasUid(newViewModel);
                var v = CreateView(newViewModel, location);
                var owner = GetOwnerWindow(ownerViewModel);
                SetupDisposableHandling(newViewModel, v);
                return (v, owner);
            });

            void CloseHandler(object? _, TResult __) => view.Close();
            void ClosedHandler(object? _, EventArgs __)
            {
                newViewModel.Close -= CloseHandler;
                newViewModel.TrySetDefaultResult();
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                newViewModel.Close += CloseHandler;
                view.Closed += ClosedHandler;
            });

            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => view.ShowDialog(ownerView));
                return await newViewModel.Result.ConfigureAwait(false);
            }
            finally
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    view.Closed -= ClosedHandler;
                    newViewModel.Close -= CloseHandler;
                });
            }
        }

        private IClassicDesktopStyleApplicationLifetime GetDesktopLifetime()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop;
            }
            throw new InvalidOperationException("Desktop application lifetime not found");
        }

        public async Task<bool> CloseViewForViewModelAsync<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IUnique
        {
            return await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var desktop = GetDesktopLifetime();
                var window = desktop.Windows.FirstOrDefault(w =>
                    w.DataContext is IUnique ctx && ctx.Uid == viewModel.Uid);

                if (window != null)
                {
                    window.Close();
                    return true;
                }

                return false;
            });
        }

        public void RegisterViewType<TViewModel, TView>()
            where TViewModel : class, IUnique
            where TView : Window
        {
            _viewTypeCache[typeof(TViewModel)] = typeof(TView);
        }

        private void EnsureViewModelHasUid<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IUnique
        {
            if (viewModel.Uid == Guid.Empty)
            {
                viewModel.Uid = _guidProvider.GetUID();
            }
        }

        private void SetupDisposableHandling<TViewModel>(
            TViewModel viewModel,
            Window window)
            where TViewModel : class, IUnique
        {
            void ClosedHandler(object? sender, EventArgs e)
            {
                window.Closed -= ClosedHandler;
                _guidProvider.ReleaseGuid(viewModel.Uid);
                Debug.WriteLine($"*** Releasing Guid for {viewModel.GetType().Name} (UID: {viewModel.Uid})");
                if (viewModel is IDisposable disposable)
                {
                    disposable.Dispose();
                    Debug.WriteLine($"*** Releasing resources for {viewModel.GetType().Name} (UID: {viewModel.Uid})");
                }
            };
            window.Closed += ClosedHandler;
        }

        private Window GetOwnerWindow<TViewModel>(TViewModel? ownerViewModel)
            where TViewModel : class, IUnique
        {
            if (ownerViewModel != null)
            {
                var desktop = GetDesktopLifetime();
                return desktop.Windows.FirstOrDefault(win =>
                    win.DataContext is IUnique ctx && ctx.Uid == ownerViewModel.Uid)
                    ?? throw new InvalidOperationException(
                        $"Window for ViewModel {typeof(TViewModel).Name} not found");
            }
            return GetMainWindow();
        }

        private Window GetMainWindow()
        {
            var desktop = GetDesktopLifetime();

            return desktop.MainWindow
                ?? throw new InvalidOperationException("MainWindow is not initialized.");
        }

        private Window CreateView<TViewModel>(
            TViewModel viewModel,
            WindowStartupLocation location)
            where TViewModel : class
        {
            var viewType = GetViewType(viewModel);
            var view = Activator.CreateInstance(viewType) as Window
                ?? throw new InvalidOperationException($"Failed to create {viewType.Name}");

            view.WindowStartupLocation = location;
            view.DataContext = viewModel;
            return view;
        }

        private static Type? DefaultViewTypeResolver(Type viewModelType)
        {
            var viewType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t =>
                    t.GetCustomAttribute<ViewForAttribute>()?.ViewModelType == viewModelType
                    && typeof(Window).IsAssignableFrom(t));

            if (viewType != null)
                return viewType;

            viewType = Assembly.GetAssembly(viewModelType)?
                .GetType(viewModelType.FullName!.Replace("ViewModel", "View"));

            return viewType;
        }

        private Type GetViewType<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            var viewModelType = viewModel.GetType();

            return _viewTypeCache.GetOrAdd(viewModelType, vmType =>
            {
                var viewType = _viewTypeResolver(vmType);

                if (viewType == null || !typeof(Window).IsAssignableFrom(viewType))
                {
                    throw new TypeLoadException(
                        $"View for {vmType.Name} not found. " +
                        $"Either register mapping or follow naming convention.");
                }

                return viewType;
            });
        }
    }
}
