using R3;

namespace Hexblick.ViewModels;

internal sealed partial class TabItemViewModel :
    IDisposable
{
    public BindableReactiveProperty<string> Title { get; }

    public BindableReactiveProperty<bool> IsDirty { get; }

    public bool IsNewDocument { get; }

    private readonly CompositeDisposable _disposable = [];

    public TabItemViewModel(
        bool isNewDocument = true)
    {
        this.IsNewDocument = isNewDocument;
        this.Title = new BindableReactiveProperty<string>("無題").AddTo(this._disposable);
        this.IsDirty = new BindableReactiveProperty<bool>().AddTo(this._disposable);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
