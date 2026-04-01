using System;

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
public sealed partial class App :
    IServiceProvider
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;

        this.InitializeComponent();

        this.DispatcherShutdownMode = DispatcherShutdownMode.OnLastWindowClose;
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var window = this._serviceProvider.GetRequiredService<MainWindow>();
        window.Activate();
    }

    /// <inheritdoc />
    object? IServiceProvider.GetService(Type serviceType)
    {
        return this._serviceProvider.GetService(serviceType);
    }
}
