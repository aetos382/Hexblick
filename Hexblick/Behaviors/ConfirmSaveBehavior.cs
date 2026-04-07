using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using CommunityToolkit.WinUI.Behaviors;

using Hexblick.Interactions;
using Hexblick.Windowing;

namespace Hexblick.Behaviors;

internal sealed class ConfirmSaveBehavior :
    BehaviorBase<FrameworkElement>
{
    /// <inheritdoc />
    protected override void OnAssociatedObjectLoaded()
    {
        var windowManager = Application.Current.Services.GetRequiredService<IWindowManager>();
        if (windowManager.TryGetWindowForElement(this.AssociatedObject, out var window))
        {
            var handler = window.WindowServices.GetRequiredService<IConfirmSaveRequesetHandler>();
            handler.SetXamlRoot(this.AssociatedObject.XamlRoot);
        }
    }
}
