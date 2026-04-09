using System;

using Microsoft.UI.Xaml;

using R3;

namespace Hexblick.Windowing;

internal sealed record WindowContext(
    Window Window) : IDisposable
{
    private readonly CompositeDisposable _disposables = [];

    public void TrackLifetime(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);

        this._disposables.Add(disposable);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposables.Dispose();
    }
}
