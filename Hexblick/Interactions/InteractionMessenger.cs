using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hexblick.Interactions;

internal sealed class InteractionMessenger
{
    private readonly IFileOpenPickerRequestHandler _fileOpenPickRequestHandler;

    public InteractionMessenger(
        IFileOpenPickerRequestHandler fileOpenPickRequestHandler)
    {
        ArgumentNullException.ThrowIfNull(fileOpenPickRequestHandler);

        this._fileOpenPickRequestHandler = fileOpenPickRequestHandler;
    }

    public async ValueTask<IReadOnlyCollection<FileInfo>> RequestFileOpenAsync(
        CancellationToken cancellation = default)
    {
        var request = new FileOpenPickerRequestMessage();
        var result = await this._fileOpenPickRequestHandler.InvokeAsync(request, cancellation);
        return result;
    }
}
