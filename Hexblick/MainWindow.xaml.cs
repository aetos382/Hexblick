using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hexblick;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel { get; }

    public MainWindow()
    {
        this.InitializeComponent();

        this.ViewModel = new();

        this.Closed += this.OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        this.ViewModel.Dispose();
    }
}
