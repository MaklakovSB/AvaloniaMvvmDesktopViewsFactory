using Avalonia.Controls;
using AvaloniaAppWithCommunityToolkitNET8.ViewModels;
using AvaloniaMvvmDesktopViewsFactory.Attributes;

namespace AvaloniaAppWithCommunityToolkitNET8.Views
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