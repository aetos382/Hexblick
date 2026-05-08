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

internal sealed class MultipleFileOpenPickerRequestMessage :
    InteractionMessage<IReadOnlyCollection<FileInfo>>
{
    private readonly List<string> _fileTypeFilter = [];

    public string? CommitButtonText { get; set; }

    public IList<string> FileTypeFilter => this._fileTypeFilter;

    public PickerLocationId SuggestedStartLocation { get; set; }

    public PickerViewMode ViewMode { get; set; }
}

internal interface IMultipleFileOpenPickerRequestHandler :
    IAsyncRequestHandler<MultipleFileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>,
    IRequiresWindowId
{
}

internal sealed class MultipleFileOpenPickerRequestHandler :
    IMultipleFileOpenPickerRequestHandler
{
    private Func<WindowId>? _windowIdAccessor;

    void IRequiresWindowId.SetWindowIdAccessor(Func<WindowId> accessor)
    {
        ArgumentNullException.ThrowIfNull(accessor);

        this._windowIdAccessor = accessor;
    }

    /// <inheritdoc />
    async ValueTask<IReadOnlyCollection<FileInfo>> IAsyncRequestHandlerCore<MultipleFileOpenPickerRequestMessage, IReadOnlyCollection<FileInfo>>.InvokeAsync(
        MultipleFileOpenPickerRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (this._windowIdAccessor is not { } accessor)
        {
            throw new InvalidOperationException();
        }

        var fileOpenPicker = new FileOpenPicker(accessor())
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

        request.TrySetResult(files);

        return files;
    }
}
