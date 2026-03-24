using R3;

namespace Hexblick;

internal sealed partial class TabItemViewModel :
    IDisposable
{
    public ReactiveProperty<string> Title { get; set; }

    public ReactiveProperty<bool> IsDirty { get; set; }

    private readonly CompositeDisposable _disposable = new();

    public TabItemViewModel()
    {
        this.Title = new ReactiveProperty<string>("無題").AddTo(this._disposable);
        this.IsDirty = new ReactiveProperty<bool>().AddTo(this._disposable);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
