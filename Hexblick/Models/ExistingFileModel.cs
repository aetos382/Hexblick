using System;
using System.IO;

namespace Hexblick.Models;

internal sealed partial class ExistingFileModel :
    Model
{
    private readonly FileInfo _fileInfo;

    public ExistingFileModel(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        this._fileInfo = fileInfo;
    }

    /// <inheritdoc />
    public override bool IsPersisted => true;

    /// <inheritdoc />
    public override string Title => this._fileInfo.Name;
}
