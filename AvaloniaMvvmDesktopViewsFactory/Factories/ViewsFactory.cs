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
        private readonly List<Assembly> _viewAssemblies = new List<Assembly>();
        private readonly List<Assembly> _viewModelAssemblies = new List<Assembly>();

        private bool _disposed;

        public ViewsFactory(IGuidProvider guidProvider, Assembly viewAssembly, Assembly viewModelAssembly)
        {
            _guidProvider = guidProvider;
            RegisterAssemblies(viewAssembly, viewModelAssembly);
        }

        /// <summary>
        /// Registers the assemblies containing Views and ViewModels.
        /// </summary>
        /// <param name="viewAssembly"></param>
        /// <param name="viewModelAssembly"></param>
        public void RegisterAssemblies(Assembly viewAssembly, Assembly viewModelAssembly)
        {
            _viewAssemblies.Add(viewAssembly);
            _viewModelAssemblies.Add(viewModelAssembly);

            var viewTypes = viewAssembly.GetTypes()
                .Where(t => typeof(Window).IsAssignableFrom(t))
                .ToList();

            var viewModelTypes = viewModelAssembly.GetTypes()
                .Where(t => t.GetInterfaces().Contains(typeof(IUnique)))
                .ToList();

            // 1. Search by ViewFor attribute.
            foreach (var viewType in viewTypes)
            {
                var attr = viewType.GetCustomAttribute<ViewForAttribute>();
                if (attr != null && attr.ViewModelType != null)
                {
                    // Check if there is already a mapping.
                    if (!_viewTypeCache.TryGetValue(attr.ViewModelType, out _))
                    {
                        _viewTypeCache.Add(attr.ViewModelType, viewType);
                    }
                    else
                    {
                        Debug.WriteLine($"[{nameof(ViewsFactory)}] Warning: Duplicate ViewFor mapping for {attr.ViewModelType.Name}.");
                    }
                }
            }

            // 2. Search by naming convention.
            foreach (var viewModelType in viewModelTypes)
            {
                // Check if there is already a mapping.
                if (_viewTypeCache.TryGetValue(viewModelType, out _))
                    continue;

                var viewName = viewModelType.Name.Replace("ViewModel", "View");
                var viewType = viewTypes.FirstOrDefault(t => t.Name == viewName);

                if (viewType != null)
                {
                    _viewTypeCache.Add(viewModelType, viewType);
                }
            }
        }

        /// <summary>
        /// Creates a main view for the application.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="mainViewModel"></param>
        /// <returns></returns>
        public Window CreateMainView<TViewModel>(TViewModel mainViewModel)
            where TViewModel : class, IUnique
        {
            EnsureViewModelHasUid(mainViewModel);
            var window = CreateView(mainViewModel, WindowStartupLocation.Manual);
            SetupDisposableHandling(mainViewModel, window);
            Debug.WriteLine($"[{nameof(ViewsFactory)}] Created MainView for {mainViewModel.GetType().Name} (UID: {mainViewModel.Uid}).");
            return window;
        }

        /// <summary>
        /// Shows a non-modal window for the given ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="newViewModel"></param>
        /// <param name="location"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Shows a modal view for the given ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="newViewModel"></param>
        /// <param name="location"></param>
        /// <param name="ownerViewModel"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Shows a dialog view for the given ViewModel and returns a result.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="newViewModel"></param>
        /// <param name="location"></param>
        /// <param name="ownerViewModel"></param>
        /// <returns></returns>
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
                        Debug.WriteLine($"[{nameof(ViewsFactory)}] Error during dialog cleanup: {ex}.");
                    }
                });
            }
        }

        /// <summary>
        /// Closes the view associated with the given ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        /// <returns></returns>
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
                    Debug.WriteLine($"[{nameof(ViewsFactory)}] Closed View for {viewModel.GetType().Name} (UID: {viewModel.Uid}).");
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Gets the main window of the application.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Window GetMainWindow()
        {
            var desktop = GetDesktopLifetime();

            return desktop.MainWindow
                ?? throw new InvalidOperationException($"[{nameof(ViewsFactory)}] MainWindow is not initialized.");
        }

        /// <summary>
        /// Gets the owner window for a modal view based on the provided ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="ownerViewModel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Window GetOwnerWindow<TViewModel>(TViewModel? ownerViewModel)
            where TViewModel : class, IUnique
        {
            if (ownerViewModel != null)
            {
                var desktop = GetDesktopLifetime();
                return desktop.Windows.FirstOrDefault(win =>
                    win.DataContext is IUnique ctx && ctx.Uid == ownerViewModel.Uid)
                    ?? throw new InvalidOperationException(
                        $"[{nameof(ViewsFactory)}] Window for ViewModel {typeof(TViewModel).Name} not found.");
            }
            return GetMainWindow();
        }

        /// <summary>
        /// Gets the current desktop application lifetime.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private IClassicDesktopStyleApplicationLifetime GetDesktopLifetime()
        {
            if (Avalonia.Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime desktop)
            {
                return desktop;
            }
            throw new InvalidOperationException($"[{nameof(ViewsFactory)}] Desktop application lifetime not found.");
        }

        /// <summary>
        /// Ensures that the ViewModel has a unique identifier (UID).
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        private void EnsureViewModelHasUid<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IUnique
        {
            if (viewModel.Uid == Guid.Empty)
            {
                viewModel.Uid = _guidProvider.GetUID();
            }
        }

        /// <summary>
        /// Sets up handling for disposable resources when the view is closed.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="window"></param>
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
                            Debug.WriteLine($"[{nameof(ViewsFactory)}] Releasing Guid for {vm.GetType().Name} (UID: {vm.Uid}).");
                        }

                        if (vm is IDisposable disposable)
                        {
                            disposable.Dispose();
                            Debug.WriteLine($"[{nameof(ViewsFactory)}] Disposed {vm.GetType().Name} (UID: {vm.Uid}).");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[{nameof(ViewsFactory)}] Error during cleanup: {ex}.");
                }
            }
            ;

            window.Closed += ClosedHandler;
        }

        /// <summary>
        /// Creates a view for the given ViewModel with the specified startup location.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        /// <param name="location"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Window CreateView<TViewModel>(
            TViewModel viewModel,
            WindowStartupLocation location)
            where TViewModel : class, IUnique
        {
            var viewType = GetViewType(viewModel);
            var view = Activator.CreateInstance(viewType) as Window
                ?? throw new InvalidOperationException($"[{nameof(ViewsFactory)}] Failed to create {viewType.Name}.");

            Debug.WriteLine($"[{nameof(ViewsFactory)}] Creating View for {viewModel.GetType().Name} (UID: {viewModel.Uid}).");
            view.WindowStartupLocation = location;
            view.DataContext = viewModel;
            return view;
        }

        /// <summary>
        /// Gets the view type for the given ViewModel.
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Type GetViewType<TViewModel>(TViewModel viewModel) where TViewModel : class
        {
            var viewModelType = viewModel.GetType();

            if (!_viewModelAssemblies.Contains(viewModelType.Assembly))
            {
                var registeredAssemblies = string.Join(", ", _viewModelAssemblies.Select(a => a.GetName().Name));
                throw new InvalidOperationException(
                    $"[{nameof(ViewsFactory)}] ViewModel {viewModelType.Name} is not from any registered ViewModel assembly. " +
                    $"Registered ViewModel assemblies: {registeredAssemblies}");
            }

            if (_viewTypeCache.TryGetValue(viewModelType, out var viewType))
            {
                return viewType;
            }

            var viewAssemblyNames = string.Join(", ", _viewAssemblies.Select(a => a.GetName().Name));
            throw new InvalidOperationException(
                $"[{nameof(ViewsFactory)}] Could not find View for {viewModelType.Name}. " +
                $"Make sure the View exists in one of the registered View assemblies: {viewAssemblyNames} " +
                $"and either follows the naming convention ('ViewModel' -> 'View') " +
                $"or is marked with [ViewFor(typeof(YourViewModel))] attribute.");
        }

        /// <summary>
        /// Disposes the ViewsFactory, releasing resources.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Debug.WriteLine($"[{nameof(ViewsFactory)}] Disposing ViewsFactory.");

                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Finalizer for ViewsFactory.
        /// </summary>
        ~ViewsFactory()
        {
            Debug.WriteLine($"[{nameof(ViewsFactory)}] ViewsFactory finalized without being disposed!");
            Dispose();
        }
    }
}
