using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hexblick.Interactions;

internal sealed class InteractionMessenger
{
    private readonly IMultipleFileOpenPickerRequestHandler _fileOpenPickRequestHandler;
    private readonly IConfirmSaveRequestHandler _confirmSaveRequesetHandler;

    public InteractionMessenger(
        IMultipleFileOpenPickerRequestHandler fileOpenPickRequestHandler,
        IConfirmSaveRequestHandler confirmSaveRequesetHandler)
    {
        ArgumentNullException.ThrowIfNull(fileOpenPickRequestHandler);
        ArgumentNullException.ThrowIfNull(confirmSaveRequesetHandler);

        this._fileOpenPickRequestHandler = fileOpenPickRequestHandler;
        this.ConfirmSaveRequesetHandler = confirmSaveRequesetHandler;
    }

    public async ValueTask<IReadOnlyCollection<FileInfo>> RequestMultipleFileOpenAsync(
        MultipleFileOpenPickerRequestMessage message,
        CancellationToken cancellation = default)
    {
        var result = await this._fileOpenPickRequestHandler.InvokeAsync(message, cancellation);
        return result;
    }

    public async ValueTask<SaveConfirmationResult> ConfirmSaveAsync(
        IReadOnlyCollection<string> titles,
        CancellationToken cancellationToken = default)
    {
        var message = new ConfirmSaveMessage(titles);
        var result = await this.ConfirmSaveRequesetHandler.InvokeAsync(message, cancellationToken);
        return result;
    }

    public IConfirmSaveRequestHandler ConfirmSaveRequesetHandler { get; }
}
