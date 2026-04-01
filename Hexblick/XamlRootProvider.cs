using System;

using Microsoft.UI.Xaml;

namespace Hexblick;

internal interface IXamlRootProvider
{
    XamlRoot? XamlRoot { get; }
}

internal sealed class WindowXamlRootProvider :
    IXamlRootProvider
{
    private readonly Window _window;

    public WindowXamlRootProvider(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        this._window = window;
    }

    /// <inheritdoc />
    public XamlRoot? XamlRoot => this._window.Content.XamlRoot;
}
