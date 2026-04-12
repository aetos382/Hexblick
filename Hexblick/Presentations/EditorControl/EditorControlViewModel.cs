using System;
using System.Threading.Tasks;

using Microsoft.UI.Xaml.Media;

using ObservableCollections;

using R3;

using Hexblick.Interactions;
using Hexblick.Models;

namespace Hexblick.Presentations;

internal sealed partial class EditorControlViewModel :
    IDisposable
{
    private readonly Model _model;

    public BindableReactiveProperty<ImageSource> Icon { get; }

    public BindableReactiveProperty<string> Title { get; }

    public BindableReactiveProperty<bool> IsDirty { get; }

    private readonly ObservableList<HexRowViewModel> _rows;

    public INotifyCollectionChangedSynchronizedViewList<HexRowViewModel> Rows { get; }

    public Observable<Unit> ClosedEvent => this._closedEvent;

    public InteractionMessenger InteractionMessenger { get; }

#pragma warning disable CA2213
    private readonly Subject<Unit> _closedEvent;
#pragma warning restore CA2213

    public bool IsNewDocument { get; }

    private readonly CompositeDisposable _disposable = [];

    public EditorControlViewModel(
        Model model,
        InteractionMessenger messenger,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._model = model;
        this.IsNewDocument = !model.IsPersisted;

        this.Icon = new BindableReactiveProperty<ImageSource>().AddTo(this._disposable);
        this.Title = new BindableReactiveProperty<string>(model.Title).AddTo(this._disposable);
        this.IsDirty = new BindableReactiveProperty<bool>().AddTo(this._disposable);

        this._rows =
        [
            new()
            {
                Offset = "00000000",
                Hex = "01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F",
                Text = "................"
            },
            new()
            {
                Offset = "00000010",
                Hex = "01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F",
                Text = "................"
            },
            new()
            {
                Offset = "00000020",
                Hex = "01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F",
                Text = "................"
            },
        ];

        this.Rows = this._rows.ToNotifyCollectionChangedSlim().AddTo(this._disposable);

        this._closedEvent = new Subject<Unit>().AddTo(this._disposable);
        this.InteractionMessenger = messenger;
    }

    public async ValueTask SaveAsync()
    {
    }

    internal async ValueTask CloseAsync()
    {
        if (this.IsDirty.Value)
        {
            var result = await this.InteractionMessenger.ConfirmSaveAsync([this.Title.Value]);
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

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }

    public void RegisterDisposable(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);

        this._disposable.Add(disposable);
    }
}
