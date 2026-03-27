namespace Hexblick.Models;

internal sealed class ModelFactory
{
    public async ValueTask<Model> OpenFileAsync(
        FileInfo file,
        CancellationToken cancellationToken)
    {
        return new ExisingFileModel();
    }
}
