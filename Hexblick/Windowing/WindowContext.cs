using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using R3;

namespace Hexblick.Windowing;

internal sealed record WindowContext(
    Window Window,
    IServiceScope ServiceScope,
    CompositeDisposable Disposables) : IDisposable
{
    public WindowContext(Window window, IServiceScope serviceScope)
        : this(window, serviceScope, [])
    {
    }

    private bool _disposed;

    /// <inheritdoc />
    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;

        this.ServiceScope.Dispose();
        this.Disposables.Dispose();
    }
}
