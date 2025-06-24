using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;

namespace NET8AvaloniaApplicationSample.ViewModels
{
    public class ModalWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public ModalWindowViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            _disposables.Dispose();

            _isDisposed = true;
            Debug.WriteLine($"[{nameof(ModalWindowViewModel)}] The Dispose method is complete for {nameof(ModalWindowViewModel)}, Guid {Uid}.");
        }
    }
}
