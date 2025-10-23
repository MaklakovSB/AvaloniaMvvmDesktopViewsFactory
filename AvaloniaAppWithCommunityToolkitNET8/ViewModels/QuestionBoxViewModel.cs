using AvaloniaAppWithCommunityToolkitNET8.Enums;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    internal class QuestionBoxViewModel : ViewModelBase, IDisposable, ICloseable<QuestionBoxResult>
    {
        private readonly TaskCompletionSource<QuestionBoxResult> _tcs = new();
        private readonly List<IDisposable> _disposables = new();
        private bool _isDisposed;

        public event EventHandler<QuestionBoxResult> Close;

        public Task<QuestionBoxResult> Result => _tcs.Task;

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }
        private string _message = string.Empty;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title = "Message";

        public AsyncRelayCommand OkCommand { get; }
        public AsyncRelayCommand CancelCommand { get; }

        public QuestionBoxViewModel(string message = null!, string title = null!)
        {
            if (!string.IsNullOrEmpty(title))
                Title = title;

            if (!string.IsNullOrEmpty(message))
                Message = message;

            OkCommand = new AsyncRelayCommand(OkMethod);
            CancelCommand = new AsyncRelayCommand(CancelMethod);
        }

        private Task OkMethod()
        {
            RaiseClose(QuestionBoxResult.Ok);
            return Task.CompletedTask;
        }

        private Task CancelMethod()
        {
            RaiseClose(QuestionBoxResult.Cancel);
            return Task.CompletedTask;
        }

        private void RaiseClose(QuestionBoxResult result)
        {
            if (_tcs.TrySetResult(result))
            {
                Close?.Invoke(this, result);
            }
        }

        public void TrySetDefaultResult()
        {
            if (_isDisposed) return;

            if (!_tcs.Task.IsCompleted)
            {
                _tcs.TrySetResult(QuestionBoxResult.Cancel);
                Debug.WriteLine($"[{nameof(QuestionBoxViewModel)}] The TrySetResult method is complete for {nameof(QuestionBoxViewModel)}.");
            }
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Clearing the OnClose event.
            Close = null!;

            TrySetDefaultResult();

            // Release of all subscriptions.
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
            _disposables.Clear();

            // Cancelling TaskCompletionSource.
            if (!_tcs.Task.IsCompleted)
            {
                _tcs.TrySetCanceled();
            }

            // Suppress the call to the finalizer.
            GC.SuppressFinalize(this);

            _isDisposed = true;
            Debug.WriteLine($"[{nameof(QuestionBoxViewModel)}] The Dispose method is complete for {nameof(QuestionBoxViewModel)}, Guid {Uid}.");
        }
    }
}