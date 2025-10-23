using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    internal class AttributeBindingViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly List<IDisposable> _disposables = new();
        private bool _isDisposed;

        public AttributeBindingViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));
        }

        // Method for adding disposable objects.
        protected void AddDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();

            _isDisposed = true;
            Debug.WriteLine($"[{nameof(AttributeBindingViewModel)}] The Dispose method is complete for {nameof(AttributeBindingViewModel)}, Guid {Uid}.");
        }
    }
}
