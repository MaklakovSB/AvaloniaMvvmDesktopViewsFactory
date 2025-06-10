using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using System;
using ReactiveUI;

namespace AvaloniaApplicationSample.ViewModels
{
    public class ViewModelBase : ReactiveObject, IUnique
    {
        public Guid Uid
        {
            get => _uid;
            set
            {
                if (_uid != Guid.Empty)
                    throw new InvalidOperationException("Uid is already assigned and cannot be changed.");

                if (value == Guid.Empty)
                    throw new ArgumentException("Uid must not be empty.", nameof(value));

                this.RaiseAndSetIfChanged(ref _uid, value);
            }
        }
        public Guid _uid;
    }
}
