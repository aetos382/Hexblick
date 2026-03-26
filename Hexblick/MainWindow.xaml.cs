using Hexblick.Hosting;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using R3;

namespace Hexblick;

internal sealed partial class MainWindow :
    Window,
    IDisposable
{
    private MainWindowViewModel ViewModel { get; }
    private readonly IWindowManager _windowManager;

    private readonly CompositeDisposable _disposable = [];

    public MainWindow(
        MainWindowViewModel viewModel,
        IWindowManager windowManager)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(windowManager);

        this.InitializeComponent();

        this._windowManager = windowManager;
        this.ViewModel = viewModel;

        this.Closed += this.OnClosed;
        this.TabView.TabCloseRequested += this.TabView_OnTabCloseRequested;
        this.ExitMenuItem.Click += this.ExitMenuItem_OnClick;
        this.NewWindowMenuItem.Click += this.NewWindowMenuItem_Click;
    private void NewWindowMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var newWindow = this._windowManager.Create<MainWindow>();
        newWindow.Activate();
    }

    private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
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

    public void Dispose()
    {
        this._disposable.Dispose();
    }
}
