using Avalonia.Controls;

namespace AvaloniaMvvmDesktopViewsFactory.Interfaces
{
    public interface IViewsFactory
    {
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

        //public void RegisterViewType<TViewModel, TView>()
        //    where TViewModel : class, IUnique where TView : Window;
    }
}
