using Avalonia.Controls;
using AvaloniaMvvmDesktopViewsFactory.Attributes;
using NET8AvaloniaApplicationSample.ViewModels;

namespace NET8AvaloniaApplicationSample.Views
{
    [ViewFor(typeof(AttributeBindingViewModel))]
    public partial class ModalWindow : Window
    {
        public ModalWindow()
        {
            InitializeComponent();
        }
    }
}