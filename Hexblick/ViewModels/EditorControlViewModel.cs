using Hexblick.Models;

using R3;

namespace Hexblick.ViewModels;

internal sealed partial class EditorControlViewModel :
    IDisposable
{
    private readonly Model _model;
    public BindableReactiveProperty<string> Title { get; }

    public BindableReactiveProperty<bool> IsDirty { get; }

    public bool IsNewDocument { get; }

    private readonly CompositeDisposable _disposable = [];

    public EditorControlViewModel(
        Model model)
    {
        this._model = model;
        this.IsNewDocument = !model.IsPersisted;
        this.Title = new BindableReactiveProperty<string>(model.Title).AddTo(this._disposable);
        this.IsDirty = new BindableReactiveProperty<bool>().AddTo(this._disposable);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
