using System;

using Microsoft.UI.Xaml;

namespace Hexblick.Interactions;

internal interface IRequiresXamlRoot
{
    void SetXamlRootAccessor(Func<XamlRoot> accessor);
}
