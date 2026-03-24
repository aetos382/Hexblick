using ObservableCollections;

using R3;

namespace Hexblick;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    public ReactiveCommand AddTabButtonCommand { get; }

    public ReactiveCommand<TabItemViewModel> CloseTabCommand { get; }

    private readonly ObservableList<TabItemViewModel> _tabItems = new();

    public NotifyCollectionChangedSynchronizedViewList<TabItemViewModel> TabItems { get; }

    private readonly CompositeDisposable _disposable = new();

    public MainWindowViewModel()
    {
        this.AddTabButtonCommand = new ReactiveCommand()
            .AddTo(this._disposable);

        this.AddTabButtonCommand
            .Subscribe(_ => this.OnAddTab())
            .AddTo(this._disposable);

        this.CloseTabCommand = new ReactiveCommand<TabItemViewModel>()
            .AddTo(this._disposable);

        this.CloseTabCommand
            .Subscribe(this.OnCloseTab)
            .AddTo(this._disposable);

        this.TabItems = this._tabItems
            .ToNotifyCollectionChangedSlim()
            .AddTo(this._disposable);

        this._tabItems.Add(new());
    }

    private void OnAddTab()
    {
        this._tabItems.Add(new());
    }

    private void OnCloseTab(TabItemViewModel item)
    {
        if (this.TabItems.Remove(item))
        {
            item.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
