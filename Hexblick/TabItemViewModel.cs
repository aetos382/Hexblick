using R3;

namespace Hexblick;

internal sealed partial class TabItemViewModel :
    IDisposable
{
    private readonly CompositeDisposable _disposable = new();

    public TabItemViewModel()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
