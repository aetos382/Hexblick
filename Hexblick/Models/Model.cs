using System;

namespace Hexblick.Models;

internal abstract class Model : IDisposable
{
    public abstract bool IsPersisted { get; }

    public abstract string Title { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }
}
