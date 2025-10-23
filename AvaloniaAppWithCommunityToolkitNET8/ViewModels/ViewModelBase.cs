using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    public class ViewModelBase : ObservableObject, IUnique
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

                SetProperty(ref _uid, value);
            }
        }
        public Guid _uid;
    }
}
