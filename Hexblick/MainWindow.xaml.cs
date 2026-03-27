using Windows.Foundation.Collections;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;

using R3;

using Hexblick.ViewModels;

namespace Hexblick;

internal sealed partial class MainWindow :
    IDisposable
{
    private MainWindowViewModel ViewModel { get; }

    private ReactiveCommand OpenFileCommand { get; }

    private readonly CompositeDisposable _disposables = [];

    public MainWindow(
        MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.InitializeComponent();

        this.ViewModel = viewModel;

        this.TabView.TabItemsChanged += this.TabView_OnTabItemsChanged;
        this.TabView.TabCloseRequested += this.TabView_OnTabCloseRequested;
        this.ExitMenuItem.Click += this.ExitMenuItem_OnClick;

        this.OpenFileCommand = new ReactiveCommand((_, cancellationToken) => this.OnFileOpenAsync(cancellationToken))
            .AddTo(this._disposables);
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

    private async ValueTask OnFileOpenAsync(CancellationToken cancellationToken)
    {
        var filePicker = new FileOpenPicker(this.AppWindow.Id)
        {
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
            ViewMode = PickerViewMode.List
        };

        var result = await filePicker.PickMultipleFilesAsync();
        if (result is [])
        {
            return;
        }

        var files = result.Select(static x => new FileInfo(x.Path)).ToArray();

        await this.ViewModel.OpenFilesAsync(files, cancellationToken);
    }

    private void ExitMenuItem_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void TabView_OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
    {
        if (args.Item is ViewModels.TabItemViewModel item)
        {
            this.ViewModel.CloseTabCommand.Execute(item);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposables.Dispose();
    }
}
