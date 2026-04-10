using System;

using Microsoft.UI;

namespace Hexblick.Interactions;

internal interface IRequiresWindowId
{
    void SetWindowIdAccessor(Func<WindowId> accessor);
}
