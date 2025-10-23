using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    internal class ModalWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly List<IDisposable> _disposables = new();
        private bool _isDisposed;

        public ModalWindowViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();

            _isDisposed = true;
            Debug.WriteLine($"[{nameof(ModalWindowViewModel)}] The Dispose method is complete for {nameof(ModalWindowViewModel)}, Guid {Uid}.");
        }
    }
}
