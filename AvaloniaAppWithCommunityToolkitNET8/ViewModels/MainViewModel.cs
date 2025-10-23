using AvaloniaAppWithCommunityToolkitNET8.Enums;
using AvaloniaMvvmDesktopViewsFactory.Interfaces;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AvaloniaAppWithCommunityToolkitNET8.ViewModels
{
    internal class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly IViewsFactory _viewsService;
        private readonly List<IDisposable> _disposables = new();
        private bool _isDisposed;

        public RelayCommand OpenQuestionBoxCommand { get; }
        public RelayCommand OpenNonICloseableModalCommand { get; }
        public RelayCommand OpenAttributeBindingViewModelCommand { get; }
        public RelayCommand OpenNonModalCommand { get; }

        public MainViewModel(IViewsFactory viewsService)
        {
            _viewsService = viewsService ?? throw new ArgumentNullException(nameof(viewsService));

            OpenQuestionBoxCommand = new RelayCommand(OpenQuestionBoxCommandMethod);
            OpenNonICloseableModalCommand = new RelayCommand(OpenNonICloseableModalCommandMethod);
            OpenAttributeBindingViewModelCommand = new RelayCommand(OpenAttributeBindingViewModelCommandMethod);
            OpenNonModalCommand = new RelayCommand(OpenNonModalCommandMethod);
        }

        private async void OpenQuestionBoxCommandMethod()
        {
            var questionBoxViewModel = new QuestionBoxViewModel("Вы уверены ... ?", "Вопрос.");
            var result = await _viewsService.ShowDialogViewWithResultAsync<QuestionBoxViewModel, QuestionBoxResult>(questionBoxViewModel);

            if (result == QuestionBoxResult.Ok)
            {
                // Action on OK.
            }
            else
            {
                // Action on Cancel.
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
            var nonModalWindowViewModel = new NonModalViewModel(_viewsService);
            _viewsService.ShowNonModalWindowAsync(nonModalWindowViewModel);
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

            Debug.WriteLine($"[{nameof(MainViewModel)}] The Dispose method is complete for {nameof(MainViewModel)}, Guid {Uid}.");
        }
    }
}