using System;
using System.Diagnostics;

namespace Hexblick;

[DebuggerDisplay("{Guid}")]
internal sealed class ServiceScopeMarker
{
    public ServiceScopeMarker()
    {
        this.Guid = Guid.NewGuid();
    }

    public Guid Guid { get; }
}
