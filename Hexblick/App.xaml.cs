using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using Hexblick.UI;
using Hexblick.Windowing;

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
        var windowFactory = this._serviceProvider.GetRequiredService<IWindowManager>();
        var window = windowFactory.CreateWindow<MainWindow>();
        window.Activate();
    }

    /// <inheritdoc />
    object? IServiceProvider.GetService(Type serviceType)
    {
        return this._serviceProvider.GetService(serviceType);
    }
}
