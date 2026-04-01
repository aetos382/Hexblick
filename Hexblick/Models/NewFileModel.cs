using System;

namespace Hexblick.Models;

internal sealed partial class NewFileModel :
    Model
{
    public NewFileModel(string title)
    {
        ArgumentNullException.ThrowIfNull(title);

        this.Title = title;
    }

    public override bool IsPersisted => false;

    /// <inheritdoc />
    public override string Title { get; }
}
