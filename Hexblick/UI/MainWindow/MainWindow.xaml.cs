using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;

using Windows.Foundation.Collections;

using R3;

using Hexblick.Localization;

namespace Hexblick.UI;

internal sealed partial class MainWindow
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
        MainWindowViewModel viewModel,
        IStringLoader stringLoader)
    {
        ArgumentNullException.ThrowIfNull(viewModel);
        ArgumentNullException.ThrowIfNull(stringLoader);

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

        this.Closed += this.OnClosed;
        this.AppWindow.Destroying += this.OnAppWindowDestroying;
    }

    private void OnAppWindowDestroying(AppWindow sender, object args)
    {
        sender.Destroying -= this.OnAppWindowDestroying;

        this._disposables.Dispose();
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

        await item.CloseAsync();
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
        // if (this._closing)
        {
            this.Closed -= this.OnClosed;
            return;
        }
    }

    private void OnExit()
    {
        this.Close();
    }
}
