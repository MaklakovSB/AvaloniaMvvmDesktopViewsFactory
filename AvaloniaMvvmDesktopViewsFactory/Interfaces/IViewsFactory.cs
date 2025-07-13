using System.Reflection;
using Avalonia.Controls;

namespace AvaloniaMvvmDesktopViewsFactory.Interfaces
{
    public interface IViewsFactory
    {
        void RegisterAssemblies(Assembly viewAssembly, Assembly viewModelAssembly);

        public Window CreateMainView<TViewModel>(
            TViewModel viewModel) where TViewModel : class, IUnique;

        public Task ShowNonModalWindowAsync<TViewModel>(
            TViewModel newViewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterScreen)
            where TViewModel : class, IUnique;

        public Task ShowModalViewAsync<TViewModel>(
            TViewModel viewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner,
            TViewModel? ownerViewModel = null) where TViewModel : class, IUnique;

        public Task<TResult> ShowDialogViewWithResultAsync<TViewModel, TResult>(
            TViewModel viewModel,
            WindowStartupLocation location = WindowStartupLocation.CenterOwner,
            TViewModel? ownerViewModel = null)
            where TViewModel : class, ICloseable<TResult>, IUnique;

        public Task<bool> CloseViewForViewModelAsync<TViewModel>(TViewModel viewModel)
            where TViewModel : class, IUnique;

        Window GetMainWindow();

        Window GetOwnerWindow<TViewModel>(TViewModel? ownerViewModel)
            where TViewModel : class, IUnique;
    }
}
