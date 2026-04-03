using System;
using System.Threading;
using System.Threading.Tasks;

using Hexblick.Interactions;
using Hexblick.Localization;
using Hexblick.Models;

using ObservableCollections;

using R3;

namespace Hexblick.UI;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    private readonly InteractionMessenger _messenger;
    private readonly IEditorControlViewModelFactory _editorControlViewModelFactory;
    private readonly IStringLoader _stringLoader;

    public ReactiveCommand NewDocumentCommand { get; }

    public ReactiveCommand OpenFilesCommand { get; }

    public ReactiveCommand<EditorControlViewModel> SaveFileCommand { get; }

    public BindableReactiveProperty<EditorControlViewModel?> ActiveDocument { get; }

    public ReactiveCommand<EditorControlViewModel> CloseEditorCommand { get; }

    private readonly ObservableList<EditorControlViewModel> _editorViewModels = [];

    public NotifyCollectionChangedSynchronizedViewList<EditorControlViewModel> EditorViewModels { get; }

    private readonly SerialDisposable _activeDocumentIsDirtySubscription = new();

    private readonly CompositeDisposable _disposable = [];

    public MainWindowViewModel(
        InteractionMessenger messenger,
        IEditorControlViewModelFactory tabItemViewModelFactory,
        IStringLoader stringLoader)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(tabItemViewModelFactory);

        this._messenger = messenger;
        this._editorControlViewModelFactory = tabItemViewModelFactory;
        this._stringLoader = stringLoader;

        this._activeDocumentIsDirtySubscription.AddTo(this._disposable);

        this.NewDocumentCommand = new ReactiveCommand(_ => this.OnNewDocument())
            .AddTo(this._disposable);

        this.OpenFilesCommand = new ReactiveCommand((_, cancellationToken) => this.OpenFilesAsync(cancellationToken))
            .AddTo(this._disposable);

        this.SaveFileCommand = new ReactiveCommand<EditorControlViewModel>(this.OnSaveFileAsync)
            .AddTo(this._disposable);

        this.ActiveDocument = new BindableReactiveProperty<EditorControlViewModel?>()
            .AddTo(this._disposable);

        this.ActiveDocument.Subscribe(this.OnActiveDocumentChanged)
            .AddTo(this._disposable);

        this.CloseEditorCommand = new ReactiveCommand<EditorControlViewModel>(this.OnCloseDocument)
            .AddTo(this._disposable);

        this.EditorViewModels = this._editorViewModels
            .ToNotifyCollectionChangedSlim()
            .AddTo(this._disposable);
    }

    private void OnNewDocument()
    {
        this._editorViewModels.Add(
            this._editorControlViewModelFactory.Create(
                new NewFileModel(
                    this._stringLoader.GetString("NewFileTitle"))));
    }

    public async ValueTask OpenFilesAsync(CancellationToken cancellationToken)
    {
        var files = await this._messenger.RequestFileOpenAsync(cancellationToken);

        var modelFactory = new ModelFactory();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var model = await modelFactory.OpenFileAsync(file, cancellationToken);

            var editorViewModel = this._editorControlViewModelFactory.Create(model);

            editorViewModel.Title.Value = file.Name;
            this._editorViewModels.Add(editorViewModel);
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

    private void OnCloseDocument(
        EditorControlViewModel item)
    {
        if (this.EditorViewModels.Remove(item))
        {
            item.Dispose();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        foreach (var editorViewModel in this._editorViewModels)
        {
            editorViewModel.Dispose();
        }

        this._disposable.Dispose();
    }
}
