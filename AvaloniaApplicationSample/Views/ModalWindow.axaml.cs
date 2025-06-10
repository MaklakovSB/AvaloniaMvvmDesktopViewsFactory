using Avalonia.Controls;
using AvaloniaApplicationSample.ViewModels;
using AvaloniaMvvmDesktopViewsFactory.Attributes;

namespace AvaloniaApplicationSample.Views
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