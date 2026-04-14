using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Hexblick.Interactions;

internal sealed class InteractionMessenger
{
    public InteractionMessenger(
        IMultipleFileOpenPickerRequestHandler fileOpenPickRequestHandler,
        IConfirmSaveRequestHandler confirmSaveRequestHandler,
        IChooseFontRequestHandler chooseFontRequestHandler)
    {
        ArgumentNullException.ThrowIfNull(fileOpenPickRequestHandler);
        ArgumentNullException.ThrowIfNull(confirmSaveRequestHandler);
        ArgumentNullException.ThrowIfNull(chooseFontRequestHandler);

        this.MultipleFileOpenPickerRequestHandler = fileOpenPickRequestHandler;
        this.ConfirmSaveRequestHandler = confirmSaveRequestHandler;
        this.ChooseFontRequestHandler = chooseFontRequestHandler;
    }

    public async ValueTask<IReadOnlyCollection<FileInfo>> RequestMultipleFileOpenAsync(
        MultipleFileOpenPickerRequestMessage message,
        CancellationToken cancellation = default)
    {
        var result = await this.MultipleFileOpenPickerRequestHandler.InvokeAsync(message, cancellation);
        return result;
    }

    public async ValueTask<SaveConfirmationResult> ConfirmSaveAsync(
        IReadOnlyCollection<string> titles,
        CancellationToken cancellationToken = default)
    {
        var message = new ConfirmSaveMessage(titles);
        var result = await this.ConfirmSaveRequestHandler.InvokeAsync(message, cancellationToken);
        return result;
    }

    public async ValueTask<object> ChooseFontAsync(
        CancellationToken cancellationToken = default)
    {
        var message = new ChooseFontMessage();
        var result = await this.ChooseFontRequestHandler.InvokeAsync(message, cancellationToken);
        return result;
    }

    public IMultipleFileOpenPickerRequestHandler MultipleFileOpenPickerRequestHandler { get; }

    public IConfirmSaveRequestHandler ConfirmSaveRequestHandler { get; }

    public IChooseFontRequestHandler ChooseFontRequestHandler { get; }
}
