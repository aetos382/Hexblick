using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using CommunityToolkit.WinUI.Behaviors;

using Hexblick.Interactions;
using Hexblick.Windowing;

namespace Hexblick.Behaviors;

internal sealed class FileOpenPickerBehavior :
    BehaviorBase<FrameworkElement>
{
    /// <inheritdoc />
    protected override void OnAssociatedObjectLoaded()
    {
        var windowManager = Application.Current.Services.GetRequiredService<IWindowManager>();

        if (windowManager.TryGetWindowForElement(this.AssociatedObject, out var window))
        {
            var handler = window.WindowServices.GetRequiredService<IFileOpenPickerRequestHandler>();
            handler.SetWindowId(window.AppWindow.Id);
        }
    }

    /// <inheritdoc />
    protected override void OnAssociatedObjectUnloaded()
    {
    }
}
