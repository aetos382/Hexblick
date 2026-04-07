using System;
using System.Threading;
using System.Threading.Tasks;

using ObservableCollections;

using R3;

using Hexblick.Interactions;
using Hexblick.Localization;
using Hexblick.Models;
using Hexblick.Utilities;

namespace Hexblick.Presentations;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    private readonly IDocumentManager _documentManager;
    private readonly InteractionMessenger _messenger;
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
        IDocumentManager documentManager,
        InteractionMessenger messenger,
        IStringLoader stringLoader)
    {
        ArgumentNullException.ThrowIfNull(documentManager);
        ArgumentNullException.ThrowIfNull(messenger);

        this._documentManager = documentManager;
        this._messenger = messenger;
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
        var viewModel = this._documentManager.CreateDocument(
            new NewFileModel(
                this._stringLoader.GetString("NewFileTitle")));

        var subscription = viewModel.ClosedEvent.Subscribe(_ => { });

        this._disposable.Add(subscription);

        this._editorViewModels.Add(viewModel);
    }

    public async ValueTask OpenFilesAsync(CancellationToken cancellationToken)
    {
        var filePickRequest = new MultipleFileOpenPickerRequestMessage();
        var files = await this._messenger.RequestMultipleFileOpenAsync(filePickRequest, cancellationToken);

        var modelFactory = new ModelFactory();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var model = await modelFactory.OpenFileAsync(file, cancellationToken);

            var editorViewModel = this._documentManager.CreateDocument(model);

            editorViewModel.Title.Value = file.Name;
            editorViewModel.Icon.Value = await FileIconExtractor.GetFileIconAsync(file.FullName);

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
