using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;

using R3;

using Hexblick.Interactions;
using Hexblick.Models;

namespace Hexblick.UI;

internal sealed partial class EditorControlViewModel :
    IDisposable
{
    private readonly Model _model;
    private readonly InteractionMessenger _messenger;

    public BindableReactiveProperty<ImageSource> Icon { get; }

    public BindableReactiveProperty<string> Title { get; }

    public BindableReactiveProperty<bool> IsDirty { get; }

    public Observable<Unit> ClosedEvent => this._closedEvent;

    private readonly Subject<Unit> _closedEvent;

    public bool IsNewDocument { get; }

    private readonly CompositeDisposable _disposable = [];

    public EditorControlViewModel(
        Model model,
        InteractionMessenger messenger)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(messenger);

        this._model = model;
        this._messenger = messenger;
        this.IsNewDocument = !model.IsPersisted;

        this.Icon = new BindableReactiveProperty<ImageSource>().AddTo(this._disposable);
        this.Title = new BindableReactiveProperty<string>(model.Title).AddTo(this._disposable);
        this.IsDirty = new BindableReactiveProperty<bool>().AddTo(this._disposable);
        this._closedEvent = new Subject<Unit>().AddTo(this._disposable);
    }

    public async ValueTask SaveAsync()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }

    internal async ValueTask CloseAsync()
    {
        if (this.IsDirty.Value)
        {
            var result = await this._messenger.ConfirmSaveAsync([this.Title.Value]);
            if (result is SaveConfirmationResult.Cancel)
            {
                return;
            }

            if (result is SaveConfirmationResult.Save)
            {
                // TODO: save
            }
        }

        this._closedEvent.OnNext(Unit.Default);
    }
}
