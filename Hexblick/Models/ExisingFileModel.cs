namespace Hexblick.Models;

internal sealed partial class ExisingFileModel :
    Model
{
    private readonly FileInfo _fileInfo;

    public ExisingFileModel(FileInfo fileInfo)
    {
        ArgumentNullException.ThrowIfNull(fileInfo);

        this._fileInfo = fileInfo;
    }

    /// <inheritdoc />
    public override bool IsPersisted => true;

    /// <inheritdoc />
    public override string Title => this._fileInfo.Name;
}
