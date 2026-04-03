using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Foundation.Collections;

using R3;

using ZLinq;

using Hexblick.Localization;
using Hexblick.Services;

namespace Hexblick.UI;

internal sealed partial class MainWindow :
    IXamlRootProvider
{
    XamlRoot? IXamlRootProvider.XamlRoot => this.Content.XamlRoot;

    private MainWindowViewModel ViewModel { get; }

    private readonly DialogService _dialogService;

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
        this._dialogService = new DialogService(stringLoader, this);

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

        this.TrackLifetime(this._disposables);
    }

    private async ValueTask OnFileOpenAsync(CancellationToken cancellationToken)
    {
        await this.ViewModel.OpenFilesAsync(cancellationToken);
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
            var result = await this._dialogService.ShowSaveConfirmationDialogAsync([item.Title.Value]);
            if (result is SaveConfirmationResult.Save)
            {
                // TODO: save
            }
            else if (result is SaveConfirmationResult.Cancel)
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

    private bool _closing;

    private async void OnClosed(object sender, WindowEventArgs args)
    {
        if (this._closing)
        {
            return;
        }

        var dirtyDocuments = this.ViewModel.EditorViewModels
            .AsValueEnumerable()
            .Where(static x => x.IsDirty.Value)
            .Select(static x => x.Title.Value)
            .ToArray();

        if (dirtyDocuments.Length == 0)
        {
            return;
        }

        args.Handled = true;

        var result = await this._dialogService.ShowSaveConfirmationDialogAsync(dirtyDocuments);
        if (result is SaveConfirmationResult.Save)
        {
            // TODO: save
        }

        if (result is SaveConfirmationResult.Cancel)
        {
            return;
        }

        this._closing = true;
        this.Close();
    }

    private void OnExit()
    {
        this.Close();
    }
}
