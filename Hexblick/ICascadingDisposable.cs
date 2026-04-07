using System;

namespace Hexblick;

internal interface ICascadingDisposable
{
    void RegisterDisposable(IDisposable disposable);
}
