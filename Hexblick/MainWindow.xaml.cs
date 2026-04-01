using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.Storage.Pickers;

using Windows.Foundation.Collections;

using R3;

using Hexblick.Localization;
using Hexblick.ViewModels;

using ZLinq;

namespace Hexblick;

internal sealed partial class MainWindow :
    IDisposable
{
    private MainWindowViewModel ViewModel { get; }
    private readonly IStringLoader _stringLoader;

    private ReactiveCommand<TabViewTabCloseRequestedEventArgs> TabViewTabCloseCommand { get; }

    private ReactiveCommand<IVectorChangedEventArgs> TabViewTabItemsChangedCommand { get; }

    private ReactiveCommand<SelectionChangedEventArgs> TabViewSelectionChangedCommand { get; }

    private ReactiveCommand OpenFileCommand { get; }

    private ReactiveCommand ExitCommand { get; }

    private readonly SerialDisposable _activeDocumentSubscription = new();

    private readonly CompositeDisposable _disposables = [];

    public MainWindow(
        MainWindowViewModel viewModel,
        IStringLoader stringLoader)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(stringLoader);

        this.InitializeComponent();

        this.ViewModel = viewModel;
        this._stringLoader = stringLoader;

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

        this.Closed += this.OnClosed;
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

        var files = result
            .AsValueEnumerable()
            .Select(static x => new FileInfo(x.Path))
            .ToArray();

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
            var stringLoader = this._stringLoader;

            // TODO: サービス化する
            var contentDialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = stringLoader.GetString("SaveConfirmationDialog.SaveButton.Text"),
                SecondaryButtonText = stringLoader.GetString("SaveConfirmationDialog.DiscardButton.Text"),
                CloseButtonText = stringLoader.GetString("SaveConfirmationDialog.CancelButton.Text"),
                DefaultButton = ContentDialogButton.Close,
                Title = "Hexblick",
                Content = new SaveConfirmationDialog
                {
                    Items = [item.Title.Value]
                },
                XamlRoot = this.Content.XamlRoot
            };

            var result = await contentDialog.ShowAsync();
            if (result is ContentDialogResult.Primary)
            {
                // TODO: save
            }
            else if (result is ContentDialogResult.None)
            {
                return;
            }
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

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        try
        {
            var dirtyDocuments = this.ViewModel.EditorViewModels
                .AsValueEnumerable()
                .Where(static x => x.IsDirty.Value)
                .Select(static x => x.Title.Value)
                .ToArray();

            if (dirtyDocuments.Length == 0)
            {
                return;
            }

            var stringLoader = this._stringLoader;

            var contentDialog = new ContentDialog
            {
                IsPrimaryButtonEnabled = true,
                IsSecondaryButtonEnabled = true,
                PrimaryButtonText = stringLoader.GetString("SaveConfirmationDialog.SaveButton.Text"),
                SecondaryButtonText = stringLoader.GetString("SaveConfirmationDialog.DiscardButton.Text"),
                CloseButtonText = stringLoader.GetString("SaveConfirmationDialog.CancelButton.Text"),
                DefaultButton = ContentDialogButton.Close,
                Title = "Hexblick",
                Content = new SaveConfirmationDialog
                {
                    Items = dirtyDocuments
                },
                XamlRoot = this.Content.XamlRoot
            };

            var result = await contentDialog.ShowAsync();

            if (result is ContentDialogResult.Primary)
            {
                // TODO: save
            }
            else if (result is ContentDialogResult.None)
            {
                args.Handled = true;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
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
