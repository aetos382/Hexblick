using Microsoft.UI;

namespace Hexblick.Interactions;

internal interface IRequiresWindowId
{
    void SetWindowId(WindowId windowId);
}
