using Microsoft.Windows.ApplicationModel.Resources;

namespace Hexblick.Localization;

internal interface IStringLoader
{
    string GetString(string resourceId);
}

internal sealed class ResourceStringLoader : IStringLoader
{
    private readonly ResourceLoader _loader = new();

    /// <inheritdoc />
    public string GetString(string resourceId)
    {
        return this._loader.GetString(resourceId.Replace('.', '/'));
    }
}
