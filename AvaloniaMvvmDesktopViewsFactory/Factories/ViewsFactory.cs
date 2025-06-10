using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaMvvmDesktopViewsFactory.Attributes;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;

namespace AvaloniaMvvmDesktopViewsFactory.Factories
{
    public class ViewsFactory : IViewsFactory, IDisposable
    {
        private readonly IGuidProvider _guidProvider;
        private readonly ConditionalWeakTable<Type, Type> _viewTypeCache = new();
        private readonly Assembly _viewAssembly;

        private bool _disposed;

        public ViewsFactory(IGuidProvider guidProvider, Assembly viewAssembly)
        {
            _guidProvider = guidProvider;
            _viewAssembly = viewAssembly ?? throw new ArgumentNullException(nameof(viewAssembly));
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
                    try
                    {
                        view.Closed -= ClosedHandler;
                        newViewModel.Close -= CloseHandler;

                        if (view is IDisposable disposableWindow)
                        {
                            disposableWindow.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error during dialog cleanup: {ex}");
                    }
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

        private void EnsureViewModelHasUid<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IUnique
        {
            if (viewModel.Uid == Guid.Empty)
            {
                viewModel.Uid = _guidProvider.GetUID();
            }
        }

        private void SetupDisposableHandling<TViewModel>(TViewModel viewModel, Window window)
            where TViewModel : class, IUnique
        {
            var weakViewModel = new WeakReference<TViewModel>(viewModel);
            var weakGuidProvider = new WeakReference<IGuidProvider>(_guidProvider);

            void ClosedHandler(object? sender, EventArgs e)
            {
                try
                {
                    window.Closed -= ClosedHandler;

                    if (weakViewModel.TryGetTarget(out var vm))
                    {
                        if (weakGuidProvider.TryGetTarget(out var provider))
                        {
                            provider.ReleaseGuid(vm.Uid);
                            Debug.WriteLine($"*** Releasing Guid for {vm.GetType().Name} (UID: {vm.Uid})");
                        }

                        if (vm is IDisposable disposable)
                        {
                            disposable.Dispose();
                            Debug.WriteLine($"*** Disposed {vm.GetType().Name} (UID: {vm.Uid})");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during cleanup: {ex}");
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

        private Type GetViewType<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            var viewModelType = viewModel.GetType();

            return _viewTypeCache.GetValue(viewModelType, vmType =>
            {
                // 1. Поиск по атрибуту ViewFor
                var viewType = _viewAssembly.GetTypes()
                    .FirstOrDefault(t => typeof(Window).IsAssignableFrom(t) &&
                        t.GetCustomAttribute<ViewForAttribute>()?.ViewModelType == vmType);

                if (viewType != null)
                {
                    return viewType;
                }

                // 2. Поиск по полному имени с заменой "ViewModel" на "View"
                var viewName = vmType.FullName!.Replace("ViewModel", "View");
                viewType = _viewAssembly.GetType(viewName);

                if (viewType == null)
                {
                    throw new InvalidOperationException(
                        $"Could not find View for {viewModelType.Name}. " +
                        $"Make sure the View is in the assembly {_viewAssembly.FullName} " +
                        $"and either follows the naming convention (FullName with 'ViewModel' -> 'View') " +
                        $"or is marked with [ViewFor(typeof(YourViewModel))] attribute.");
                }

                return viewType;
            });
        }


        public void Dispose()
        {
            if (!_disposed)
            {
                Debug.WriteLine("Disposing ViewsFactory");

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        ~ViewsFactory()
        {
            Debug.WriteLine("ViewsFactory finalized without being disposed!");
            Dispose();
        }
    }
}
