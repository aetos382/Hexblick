using System;

using Microsoft.UI.Xaml;

namespace Hexblick;

internal interface IXamlRootProvider
{
    XamlRoot? XamlRoot { get; }
}
