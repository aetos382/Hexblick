using Microsoft.Windows.ApplicationModel.Resources;

namespace Hexblick.Localization;

internal interface IStringLoader
{
    string GetString(string resourceId);
}

internal sealed class ResourceLoaderStringLoader : IStringLoader
{
    private ResourceLoader? _loader;

    /// <inheritdoc />
    public string GetString(string resourceId)
    {
        Interlocked.CompareExchange(ref this._loader, new(), null);

        return this._loader.GetString(resourceId);
    }
}
