using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hexblick.Interactions;

internal sealed class InteractionMessenger
{
    private readonly IMultipleFileOpenPickerRequestHandler _fileOpenPickRequestHandler;

    public InteractionMessenger(
        IMultipleFileOpenPickerRequestHandler fileOpenPickRequestHandler)
    {
        ArgumentNullException.ThrowIfNull(fileOpenPickRequestHandler);

        this._fileOpenPickRequestHandler = fileOpenPickRequestHandler;
    }

    public async ValueTask<IReadOnlyCollection<FileInfo>> RequestMultipleFileOpenAsync(
        MultipleFileOpenPickerRequestMessage message,
        CancellationToken cancellation = default)
    {
        var result = await this._fileOpenPickRequestHandler.InvokeAsync(message, cancellation);
        return result;
    }
}
