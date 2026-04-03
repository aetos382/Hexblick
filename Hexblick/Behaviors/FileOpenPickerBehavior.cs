using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

using CommunityToolkit.WinUI.Behaviors;

using MessagePipe;

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
            var serviceScopeMarker = window.WindowServices.GetRequiredService<ServiceScopeMarker>();
            var handler = window.WindowServices.GetRequiredService<IAsyncRequestHandler<FileOpenPickerRequestMessage, int>>();
        }
    }

    /// <inheritdoc />
    protected override void OnAssociatedObjectUnloaded()
    {

    }

    private sealed class MessageHandler :
        IAsyncRequestHandler<FileOpenPickerRequestMessage, int>
    {
        private readonly ServiceScopeMarker _marker;

        public MessageHandler(
            ServiceScopeMarker marker)
        {
            this._marker = marker;
        }

        /// <inheritdoc />
        public async ValueTask<int> InvokeAsync(
            FileOpenPickerRequestMessage request,
            CancellationToken cancellationToken = default)
        {
            return 1;
        }
    }
}
