using System;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using ReactiveUI;

namespace NET8AvaloniaApplicationSample.ViewModels
{
    public class ViewModelBase : ReactiveObject, IUnique
    {
        public Guid Uid
        {
            get => _uid;
            set
            {
                if (_uid != Guid.Empty)
                    throw new InvalidOperationException($"[{nameof(ViewModelBase)}] Uid is already assigned and cannot be changed.");

                if (value == Guid.Empty)
                    throw new ArgumentException($"[{nameof(ViewModelBase)}] Uid must not be empty.", nameof(value));

                this.RaiseAndSetIfChanged(ref _uid, value);
            }
        }
        public Guid _uid;
    }
}
