using ObservableCollections;

using R3;

namespace Hexblick;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    public ReactiveCommand NewDocumentCommand { get; }

    public ReactiveCommand ExitCommand { get; }

    public ReactiveCommand<TabItemViewModel> CloseTabCommand { get; }

    private readonly ObservableList<TabItemViewModel> _tabItems = new();

    public NotifyCollectionChangedSynchronizedViewList<TabItemViewModel> TabItems { get; }

    private readonly CompositeDisposable _disposable = new();

    public MainWindowViewModel()
    {
        this.NewDocumentCommand = new ReactiveCommand()
            .AddTo(this._disposable);

        this.NewDocumentCommand
            .Subscribe(_ => this.OnNewDocument())
            .AddTo(this._disposable);

        this.ExitCommand = new ReactiveCommand()
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

    private void OnNewDocument()
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
        foreach (var tabItem in this._tabItems)
        {
            tabItem.Dispose();
        }

        this._disposable.Dispose();
    }
}
