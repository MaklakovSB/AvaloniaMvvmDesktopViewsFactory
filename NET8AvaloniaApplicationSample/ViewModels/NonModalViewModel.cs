using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using ReactiveUI;

namespace NET8AvaloniaApplicationSample.ViewModels
{
    public class NonModalViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public ReactiveCommand<Unit, Unit> CloseNonModalCommand { get; }

        public NonModalViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));

            CloseNonModalCommand = ReactiveCommand.Create(CloseNonModalCommandMethod).DisposeWith(_disposables);
        }

        private void CloseNonModalCommandMethod()
        {
            _viewsService.CloseViewForViewModelAsync(this);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            _disposables.Dispose();

            _isDisposed = true;
            Debug.WriteLine($"[{nameof(NonModalViewModel)}] The Dispose method is complete for {nameof(NonModalViewModel)}, Guid {Uid}.");
        }
    }
}
