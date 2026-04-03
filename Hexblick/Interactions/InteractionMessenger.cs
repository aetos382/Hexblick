using System;
using System.Threading;
using System.Threading.Tasks;

using MessagePipe;

namespace Hexblick.Interactions;

internal sealed class InteractionMessenger
{
    private readonly IAsyncRequestHandler<FileOpenPickerRequestMessage, int> _fileOpenPickRequestHandler;

    public InteractionMessenger(
        IAsyncRequestHandler<FileOpenPickerRequestMessage, int> fileOpenPickRequestHandler)
    {
        ArgumentNullException.ThrowIfNull(fileOpenPickRequestHandler);

        this._fileOpenPickRequestHandler = fileOpenPickRequestHandler;
    }

    public async ValueTask<int> RequestFileOpenAsync(
        CancellationToken cancellation = default)
    {
        var request = new FileOpenPickerRequestMessage();
        var result = await this._fileOpenPickRequestHandler.InvokeAsync(request, cancellation);
        return result;
    }
}
