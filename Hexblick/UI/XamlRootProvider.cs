using Microsoft.UI.Xaml;

namespace Hexblick.UI;

internal interface IXamlRootProvider
{
    XamlRoot? XamlRoot { get; }
}
