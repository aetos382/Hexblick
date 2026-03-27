using ObservableCollections;

using R3;

namespace Hexblick;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    private readonly ITabItemViewModelFactory _tabItemViewModelFactory;

    public ReactiveCommand NewDocumentCommand { get; }

    public ReactiveCommand<TabItemViewModel> CloseTabCommand { get; }

    private readonly ObservableList<TabItemViewModel> _tabItems = [];

    public NotifyCollectionChangedSynchronizedViewList<TabItemViewModel> TabItems { get; }

    private readonly CompositeDisposable _disposable = [];

    public MainWindowViewModel(
        ITabItemViewModelFactory tabItemViewModelFactory)
    {
        ArgumentNullException.ThrowIfNull(tabItemViewModelFactory);

        this._tabItemViewModelFactory = tabItemViewModelFactory;

        this.NewDocumentCommand = new ReactiveCommand()
            .AddTo(this._disposable);

        this.NewDocumentCommand
            .Subscribe(_ => this.OnNewDocument())
            .AddTo(this._disposable);

        this.CloseTabCommand = new ReactiveCommand<TabItemViewModel>()
            .AddTo(this._disposable);

        this.CloseTabCommand
            .Subscribe(this.OnCloseTab)
            .AddTo(this._disposable);

        this.TabItems = this._tabItems
            .ToNotifyCollectionChangedSlim()
            .AddTo(this._disposable);
    }

    private void OnNewDocument()
    {
        this._tabItems.Add(this._tabItemViewModelFactory.Create());
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
        foreach (var tabItem in this._tabItems)
        {
            tabItem.Dispose();
        }

        this._disposable.Dispose();
    }
}
