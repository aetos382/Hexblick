using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using Hexblick.Presentations;
using Hexblick.Windowing;

namespace Hexblick;

// https://github.com/microsoft/microsoft-ui-xaml/issues/10099
#pragma warning disable CA1515

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public sealed partial class App :
    IServiceProvider,
    IXamlMetadataProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IXamlMetadataProvider _internalProvider;

    IXamlType IXamlMetadataProvider.GetXamlType(string fullName)
    {
        return this._internalProvider.GetXamlType(fullName);
    }

    IXamlType IXamlMetadataProvider.GetXamlType(Type type)
    {
        return this._internalProvider.GetXamlType(type);
    }

    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
    {
        return this._internalProvider.GetXmlnsDefinitions();
    }

    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
        this._internalProvider = XamlMetadataProviderFactory.CreateProvider(this);

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
