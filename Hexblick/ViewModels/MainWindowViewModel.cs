using Hexblick.Localization;
using Hexblick.Models;

using ObservableCollections;

using R3;

namespace Hexblick.ViewModels;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    private readonly ITabItemViewModelFactory _tabItemViewModelFactory;
    private readonly IStringLoader _stringLoader;

    public ReactiveCommand NewDocumentCommand { get; }

    public ReactiveCommand<IReadOnlyList<FileInfo>> OpenFilesCommand { get; }

    public ReactiveCommand<EditorControlViewModel> SaveFileCommand { get; }

    public ReactiveProperty<EditorControlViewModel?> ActiveDocument { get; }

    public ReactiveCommand<EditorControlViewModel> CloseTabCommand { get; }

    private readonly ObservableList<EditorControlViewModel> _tabItems = [];

    public NotifyCollectionChangedSynchronizedViewList<EditorControlViewModel> TabItems { get; }

    private readonly SerialDisposable _activeDocumentIsDirtySubscription = new();

    private readonly CompositeDisposable _disposable = [];

    public MainWindowViewModel(
        ITabItemViewModelFactory tabItemViewModelFactory,
        IStringLoader stringLoader)
    {
        ArgumentNullException.ThrowIfNull(tabItemViewModelFactory);

        this._tabItemViewModelFactory = tabItemViewModelFactory;
        this._stringLoader = stringLoader;

        this._activeDocumentIsDirtySubscription.AddTo(this._disposable);

        this.NewDocumentCommand = new ReactiveCommand(_ => this.OnNewDocument())
            .AddTo(this._disposable);

        this.OpenFilesCommand = new ReactiveCommand<IReadOnlyList<FileInfo>>(this.OpenFilesAsync)
            .AddTo(this._disposable);

        this.SaveFileCommand = new ReactiveCommand<EditorControlViewModel>(this.OnSaveFileAsync)
            .AddTo(this._disposable);

        this.ActiveDocument = new ReactiveProperty<EditorControlViewModel?>()
            .AddTo(this._disposable);

        this.ActiveDocument.Subscribe(this.OnActiveDocumentChanged)
            .AddTo(this._disposable);

        this.CloseTabCommand = new ReactiveCommand<EditorControlViewModel>(this.OnCloseTab)
            .AddTo(this._disposable);

        this.TabItems = this._tabItems
            .ToNotifyCollectionChangedSlim()
            .AddTo(this._disposable);
    }

    private void OnNewDocument()
    {
        this._tabItems.Add(
            this._tabItemViewModelFactory.Create(
                new NewFileModel(
                    this._stringLoader.GetString("NewFileTitle"))));
    }

    public async ValueTask OpenFilesAsync(
        IReadOnlyList<FileInfo> files,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(files);

        var modelFactory = new ModelFactory();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var model = await modelFactory.OpenFileAsync(file, cancellationToken);

            var tabItem = this._tabItemViewModelFactory.Create(model);

            tabItem.Title.Value = file.Name;
            this._tabItems.Add(tabItem);
        }
    }

    private async ValueTask OnSaveFileAsync(
        EditorControlViewModel viewModel,
        CancellationToken cancellationToken)
    {
    }

    private void OnActiveDocumentChanged(
        EditorControlViewModel? viewModel)
    {
        if (viewModel is null)
        {
            this.SaveFileCommand.ChangeCanExecute(false);
            return;
        }

        this._activeDocumentIsDirtySubscription.Disposable = viewModel.IsDirty
            .Subscribe(isDirty => this.SaveFileCommand.ChangeCanExecute(!viewModel.IsNewDocument && isDirty));
    }

    private void OnCloseTab(EditorControlViewModel item)
    {
        if (this.TabItems.Remove(item))
        {
            item.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var tabItem in this._tabItems)
        {
            tabItem.Dispose();
        }

        this._disposable.Dispose();
    }
}
