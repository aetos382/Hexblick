using Windows.Foundation.Collections;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Hexblick;

internal sealed partial class MainWindow
{
    private MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.InitializeComponent();

        this.ViewModel = viewModel;

        this.TabView.TabItemsChanged += this.TabView_OnTabItemsChanged;
        this.TabView.TabCloseRequested += this.TabView_OnTabCloseRequested;
        this.ExitMenuItem.Click += this.ExitMenuItem_OnClick;
    }

    private void TabView_OnTabItemsChanged(TabView sender, IVectorChangedEventArgs args)
    {
        switch (args.CollectionChange)
        {
            case CollectionChange.ItemInserted:
                sender.SelectedIndex = (int)args.Index;
                break;
        }
    }

    private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is TabItemViewModel item)
        {
            this.ViewModel.CloseTabCommand.Execute(item);
        }
    }
}
