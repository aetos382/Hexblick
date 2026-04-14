using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Windowing;

using Windows.Foundation.Collections;

using Hexblick.Strings;

using R3;

namespace Hexblick.Presentations;

internal sealed partial class MainWindow
{
    private MainWindowViewModel ViewModel { get; }

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

        this.ExitCommand = new ReactiveCommand(_ => this.OnExit())
            .AddTo(this._disposables);

        this.ViewModel.ActiveDocument.Subscribe(this.OnActiveDocumentChanged)
            .AddTo(this._disposables);

        this.TabView.TabItemsChanged += this.OnTabViewTabItemsChanged;
        this.TabView.SelectionChanged += this.OnTabViewSelectionChanged;
        this.TabView.TabCloseRequested += this.OnTabViewTabCloseRequestedAsync;

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

    private void OnTabViewSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var tabViewModel = this.TabView.SelectedItem as EditorControlViewModel;
        this.ViewModel.ActiveDocument.Value = tabViewModel;
    }

    private void OnTabViewTabItemsChanged(TabView sender, IVectorChangedEventArgs args)
    {
        switch (args.CollectionChange)
        {
            case CollectionChange.ItemInserted:
                this.TabView.SelectedIndex = (int)args.Index;
                break;
        }
    }

    private async void OnTabViewTabCloseRequestedAsync(TabView tabView, TabViewTabCloseRequestedEventArgs args)
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
        if (this._closing)
        {
            this.Closed -= this.OnClosed;
            return;
        }

        var task = this.ViewModel.CloseAsync();
        if (task.IsCompleted)
        {
            args.Handled = !task.Result;
            return;
        }

        args.Handled = true;

        if (await task)
        {
            this._closing = true;
            this.Close();
        }
    }

    private void OnExit()
    {
        this.Close();
    }
}
