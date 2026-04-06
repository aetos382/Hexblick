using Microsoft.UI.Xaml;

namespace Hexblick.Interactions;

internal interface IRequiresXamlRoot
{
    void SetXamlRoot(XamlRoot xamlRoot);
}
