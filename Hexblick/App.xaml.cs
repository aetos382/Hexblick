using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Hexblick;

// https://github.com/microsoft/microsoft-ui-xaml/issues/10099
#pragma warning disable CA1515

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    private readonly IWindowManager _windowManager;
    private Window? _window;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App(
        IWindowManager windowManager)
    {
        ArgumentNullException.ThrowIfNull(windowManager);

        this._windowManager = windowManager;

        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = this._window = this._windowManager.Create<MainWindow>();
        window.Activate();
    }
}
