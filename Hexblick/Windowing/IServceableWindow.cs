using System;

using Microsoft.Extensions.DependencyInjection;

namespace Hexblick.Windowing;

internal interface IServceableWindow
{
    void SetServiceScope(IServiceScope scope);

    IServiceProvider WindowServices { get; }

    T TrackLifetime<T>(T item) where T : IDisposable;
}
