using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;

using CommunityToolkit.WinUI.Behaviors;

using MessagePipe;

using Hexblick.Interactions;

namespace Hexblick.Behaviors;

internal sealed class FileOpenPickerBehavior :
    BehaviorBase<UIElement>
{
    private sealed class MessageHandler :
        IAsyncRequestHandler<FileOpenPickerRequestMessage, int>
    {
        /// <inheritdoc />
        public async ValueTask<int> InvokeAsync(
            FileOpenPickerRequestMessage request,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return 1;
        }
    }
}
