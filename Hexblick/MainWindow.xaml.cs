using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using R3;

namespace Hexblick;

internal sealed partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel { get; }

    private readonly CompositeDisposable _disposable = new();

    public MainWindow()
    {
        this.InitializeComponent();

        this.ViewModel = new MainWindowViewModel()
            .AddTo(this._disposable);

        this.ViewModel.ExitCommand
            .Subscribe(_ => this.Close())
            .AddTo(this._disposable);

        this.Closed += this.OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        this._disposable.Dispose();
    }

    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabItemViewModel item)
        {
            this.ViewModel.CloseTabCommand.Execute(item);
        }
    }
}
