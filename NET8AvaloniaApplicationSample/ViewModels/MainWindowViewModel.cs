using ReactiveUI;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive;
using System;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using NET8AvaloniaApplicationSample.Enums;

namespace NET8AvaloniaApplicationSample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly CompositeDisposable _disposables = new();
        private bool _isDisposed;

        public ReactiveCommand<Unit, Unit> OpenQuestionBoxCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenNonICloseableModalCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenAttributeBindingViewModelCommand { get; }
        public ReactiveCommand<Unit, Unit> OpenNonModalCommand { get; }

        public MainWindowViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));

            OpenQuestionBoxCommand = ReactiveCommand.Create(OpenQuestionBoxCommandMethod).DisposeWith(_disposables);
            OpenNonICloseableModalCommand = ReactiveCommand.Create(OpenNonICloseableModalCommandMethod).DisposeWith(_disposables);
            OpenAttributeBindingViewModelCommand = ReactiveCommand.Create(OpenAttributeBindingViewModelCommandMethod).DisposeWith(_disposables);
            OpenNonModalCommand = ReactiveCommand.Create(OpenNonModalCommandMethod).DisposeWith(_disposables);
        }

        private async void OpenQuestionBoxCommandMethod()
        {
            var questionBoxViewModel = new QuestionBoxViewModel("Ну как?", "Вопрос.");
            var result = await _viewsService.ShowDialogViewWithResultAsync<QuestionBoxViewModel, QuestionBoxResult>(questionBoxViewModel);

            if (result == QuestionBoxResult.Ok)
            {
            }
            else
            {
            }
        }

        private async void OpenNonICloseableModalCommandMethod()
        {
            var modalWindowViewModel = new ModalWindowViewModel(_viewsService);
            await _viewsService.ShowModalViewAsync(modalWindowViewModel);
        }

        private async void OpenAttributeBindingViewModelCommandMethod()
        {
            var attributeBindingViewModel = new AttributeBindingViewModel(_viewsService);
            await _viewsService.ShowModalViewAsync(attributeBindingViewModel);
        }

        private void OpenNonModalCommandMethod()
        {
            var NonModalWindowViewModel = new NonModalViewModel(_viewsService);
            _viewsService.ShowNonModalWindowAsync(NonModalWindowViewModel);
        }

        public void Dispose()
        {
            if (_isDisposed) return;

            // Release of all subscriptions.
            _disposables.Dispose();

            _isDisposed = true;

            Debug.WriteLine($"[{nameof(MainWindowViewModel)}] The Dispose method is complete for {nameof(MainWindowViewModel)}, Guid {Uid}.");
        }
    }
}
