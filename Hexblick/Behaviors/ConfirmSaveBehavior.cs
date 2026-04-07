using System;

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
        IConfirmSaveRequesetHandler? handler = null;

        if (this.AssociatedObject is IServiceProvider sp)
        {
            handler = sp.GetService<IConfirmSaveRequesetHandler>();
        }

        if (handler is null)
        {
            var windowManager = Application.Current.Services.GetRequiredService<IWindowManager>();
            if (windowManager.TryGetWindowForElement(this.AssociatedObject, out var window))
            {
                handler = window.WindowServices.GetRequiredService<IConfirmSaveRequesetHandler>();
            }
        }

        handler!.SetXamlRoot(this.AssociatedObject.XamlRoot);
    }
}
