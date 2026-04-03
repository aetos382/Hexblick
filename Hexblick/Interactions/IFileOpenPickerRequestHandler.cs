using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI;
using Microsoft.Windows.Storage.Pickers;

using MessagePipe;

using ZLinq;

namespace Hexblick.Interactions;

internal interface IFileOpenPickerRequestHandler :
    IAsyncRequestHandler<FileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>
{
    void SetWindowId(WindowId windowId);
}

internal sealed class FileOpenPickerRequestHandler :
    IFileOpenPickerRequestHandler
{
    private WindowId? _windowId;

    void IFileOpenPickerRequestHandler.SetWindowId(WindowId windowId)
    {
        this._windowId = windowId;
    }

    /// <inheritdoc />
    async ValueTask<IReadOnlyCollection<FileInfo>> IAsyncRequestHandlerCore<FileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>.InvokeAsync(
        FileOpenPickerRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (this._windowId is not { } windowId)
        {
            throw new InvalidOperationException();
        }

        var fileOpenPicker = new FileOpenPicker(windowId);

        var result = await fileOpenPicker.PickMultipleFilesAsync();
        var files = result.AsValueEnumerable().Select(static x => new FileInfo(x.Path)).ToArray();

        return files;
    }
}
