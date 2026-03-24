using System.Collections.ObjectModel;

using R3;

namespace Hexblick;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    public ReactiveCommand AddTabButtonCommand { get; }

    public ReactiveCommand<TabItemViewModel> CloseTabCommand { get; }

    public ObservableCollection<TabItemViewModel> TabItems { get; }

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

        this.TabItems = new ObservableCollection<TabItemViewModel>()
            .AddTo(this._disposable);
    }

    private void OnAddTab()
    {
        this.TabItems.Add(new());
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
