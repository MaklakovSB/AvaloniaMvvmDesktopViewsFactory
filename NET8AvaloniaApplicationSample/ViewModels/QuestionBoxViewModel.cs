using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading.Tasks;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using NET8AvaloniaApplicationSample.Enums;
using ReactiveUI;

namespace NET8AvaloniaApplicationSample.ViewModels
{
    public class QuestionBoxViewModel : ViewModelBase, IDisposable, ICloseable<QuestionBoxResult>
    {
        private readonly TaskCompletionSource<QuestionBoxResult> _tcs = new();
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public event EventHandler<QuestionBoxResult> Close;

        public Task<QuestionBoxResult> Result => _tcs.Task;

        public string Message
        {
            get => _message;
            set => this.RaiseAndSetIfChanged(ref _message, value);
        }
        private string _message = string.Empty;

        public string Title
        {
            get => _title;
            set => this.RaiseAndSetIfChanged(ref _title, value);
        }
        private string _title = "Сообщение";

        public ReactiveCommand<Unit, Unit> OkCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

        public QuestionBoxViewModel(string message = null!, string title = null!)
        {
            if (!string.IsNullOrEmpty(title))
                Title = title;

            if (!string.IsNullOrEmpty(message))
                Message = message;

            OkCommand = ReactiveCommand.CreateFromTask(OkMethod).DisposeWith(_disposables);
            CancelCommand = ReactiveCommand.CreateFromTask(CancelMethod).DisposeWith(_disposables);
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
            _disposables.Dispose();

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
