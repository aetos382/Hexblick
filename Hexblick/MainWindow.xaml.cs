using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;

using Windows.Foundation.Collections;

using R3;

using Hexblick.ViewModels;

using WinRT.Interop;

namespace Hexblick;

internal sealed partial class MainWindow :
    IDisposable
{
    private MainWindowViewModel ViewModel { get; }

    private ReactiveCommand<TabViewTabCloseRequestedEventArgs> TabViewTabCloseCommand { get; }

    private ReactiveCommand<IVectorChangedEventArgs> TabViewTabItemsChangedCommand { get; }

    private ReactiveCommand<SelectionChangedEventArgs> TabViewSelectionChangedCommand { get; }

    private ReactiveCommand OpenFileCommand { get; }

    private ReactiveCommand ExitCommand { get; }

    private readonly SerialDisposable _activeDocumentSubscription = new();

    private readonly CompositeDisposable _disposables = [];

    public MainWindow(
        MainWindowViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.InitializeComponent();

        this.ViewModel = viewModel;

        this._activeDocumentSubscription.AddTo(this._disposables);

        this.OpenFileCommand = new ReactiveCommand((_, cancellationToken) => this.OnFileOpenAsync(cancellationToken))
            .AddTo(this._disposables);

        this.TabViewTabItemsChangedCommand = new ReactiveCommand<IVectorChangedEventArgs>(this.OnTabViewTabItemsChanged)
            .AddTo(this._disposables);

        this.TabViewSelectionChangedCommand = new ReactiveCommand<SelectionChangedEventArgs>(this.OnTabViewSelectionChanged)
            .AddTo(this._disposables);

        this.TabViewTabCloseCommand = new ReactiveCommand<TabViewTabCloseRequestedEventArgs>(this.OnTabViewTabCloseRequestedAsync)
            .AddTo(this._disposables);

        this.ExitCommand = new ReactiveCommand(_ => this.OnExit())
            .AddTo(this._disposables);

        this.ViewModel.ActiveDocument.Subscribe(this.OnActiveDocumentChanged)
            .AddTo(this._disposables);
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

    private void OnActiveDocumentChanged(EditorControlViewModel? viewModel)
    {
        if (viewModel is null)
        {
            this.SetTitle(null, false);
            return;
        }

        this._activeDocumentSubscription.Disposable = viewModel.Title
            .CombineLatest(
                viewModel.IsDirty,
                static (title, isDirty) => (Title: title, IsDirty: isDirty))
            .Subscribe(
                args => this.SetTitle(args.Title, args.IsDirty));
    }

    private void OnTabViewSelectionChanged(SelectionChangedEventArgs e)
    {
        var tabViewModel = this.TabView.SelectedItem as EditorControlViewModel;
        this.ViewModel.ActiveDocument.Value = tabViewModel;
    }

    private void OnTabViewTabItemsChanged(IVectorChangedEventArgs args)
    {
        switch (args.CollectionChange)
        {
            case CollectionChange.ItemInserted:
                this.TabView.SelectedIndex = (int)args.Index;
                break;
        }
    }

    private async ValueTask OnTabViewTabCloseRequestedAsync(
        TabViewTabCloseRequestedEventArgs args,
        CancellationToken cancellationToken)
    {
        if (args.Item is not EditorControlViewModel item)
        {
            return;
        }

        if (item.IsDirty.Value)
        {
            var contentDialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = "保存する",
                SecondaryButtonText = "保存しない",
                CloseButtonText = "キャンセル",
                DefaultButton = ContentDialogButton.Close,
                Title = "Hexblick",
                Content = new SingleFileQuerySaveDialog(),
                XamlRoot = this.Content.XamlRoot
            };

            var result = await contentDialog.ShowAsync();
        }

        this.ViewModel.CloseEditorCommand.Execute(item);
    }

    private void SetTitle(string? activeDocumentTitle, bool isDirty)
    {
        if (string.IsNullOrEmpty(activeDocumentTitle))
        {
            this.Title = "Hexblick";
        }
        else if (!isDirty)
        {
            this.Title = $"Hexblick - {activeDocumentTitle}";
        }
        else
        {
            this.Title = $"Hexblick - {activeDocumentTitle} *";
        }
    }

    private void OnExit()
    {
        this.Close();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._disposables.Dispose();
    }
}
