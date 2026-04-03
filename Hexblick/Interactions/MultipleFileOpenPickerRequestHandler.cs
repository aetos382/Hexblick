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

internal sealed class MultipleFileOpenPickerRequestMessage
{
    private readonly List<string> _fileTypeFilter = new();

    public string? CommitButtonText { get; set; }

    public IList<string> FileTypeFilter => this._fileTypeFilter;

    public PickerLocationId SuggestedStartLocation { get; set; }

    public PickerViewMode ViewMode { get; set; }
}

internal interface IMultipleFileOpenPickerRequestHandler :
    IAsyncRequestHandler<MultipleFileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>
{
    void SetWindowId(WindowId windowId);
}

internal sealed class MultipleFileOpenPickerRequestHandler :
    IMultipleFileOpenPickerRequestHandler
{
    private WindowId? _windowId;

    void IMultipleFileOpenPickerRequestHandler.SetWindowId(WindowId windowId)
    {
        this._windowId = windowId;
    }

    /// <inheritdoc />
    async ValueTask<IReadOnlyCollection<FileInfo>> IAsyncRequestHandlerCore<MultipleFileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>.InvokeAsync(
        MultipleFileOpenPickerRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (this._windowId is not { } windowId)
        {
            throw new InvalidOperationException();
        }

        var fileOpenPicker = new FileOpenPicker(windowId)
        {
            ViewMode = request.ViewMode,
            SuggestedStartLocation = request.SuggestedStartLocation
        };

        foreach (var filter in request.FileTypeFilter)
        {
            fileOpenPicker.FileTypeFilter.Add(filter);
        }

        if (request.CommitButtonText is { } commitButtonText)
        {
            fileOpenPicker.CommitButtonText = commitButtonText;
        }

        var result = await fileOpenPicker.PickMultipleFilesAsync();
        var files = result.AsValueEnumerable().Select(static x => new FileInfo(x.Path)).ToArray();

        return files;
    }
}
