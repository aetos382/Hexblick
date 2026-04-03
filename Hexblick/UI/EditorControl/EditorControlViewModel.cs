using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;

using R3;

using Hexblick.Models;

namespace Hexblick.UI;

internal sealed partial class EditorControlViewModel :
    IDisposable
{
    private readonly Model _model;

    public BindableReactiveProperty<ImageSource> Icon { get; }

    public BindableReactiveProperty<string> Title { get; }

    public BindableReactiveProperty<bool> IsDirty { get; }

    public bool IsNewDocument { get; }

    private readonly CompositeDisposable _disposable = [];

    public EditorControlViewModel(
        Model model)
    {
        this._model = model;
        this.IsNewDocument = !model.IsPersisted;

        this.Icon = new BindableReactiveProperty<ImageSource>().AddTo(this._disposable);
        this.Title = new BindableReactiveProperty<string>(model.Title).AddTo(this._disposable);
        this.IsDirty = new BindableReactiveProperty<bool>().AddTo(this._disposable);
    }

    public async ValueTask SaveAsync()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
