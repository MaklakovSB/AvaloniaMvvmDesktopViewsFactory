using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    internal class NonModalViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly List<IDisposable> _disposables = new();
        private bool _isDisposed;

        public RelayCommand CloseNonModalCommand { get; }

        public NonModalViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));

            CloseNonModalCommand = new RelayCommand(CloseNonModalCommandMethod);
        }

        private void CloseNonModalCommandMethod()
        {
            _viewsService.CloseViewForViewModelAsync(this);
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
            Debug.WriteLine($"[{nameof(NonModalViewModel)}] The Dispose method is complete for {nameof(NonModalViewModel)}, Guid {Uid}.");
        }
    }
}