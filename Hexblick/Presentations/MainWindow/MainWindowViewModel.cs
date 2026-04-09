using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using ObservableCollections;

using R3;

using ZLinq;

using Hexblick.Interactions;
using Hexblick.Localization;
using Hexblick.Models;
using Hexblick.Utilities;

namespace Hexblick.Presentations;

internal sealed partial class MainWindowViewModel :
    IDisposable
{
    public InteractionMessenger InteractionMessenger { get; }

    private readonly IStringLoader _stringLoader;

    public ReactiveCommand NewDocumentCommand { get; }

    public ReactiveCommand OpenFilesCommand { get; }

    public ReactiveCommand<EditorControlViewModel> SaveFileCommand { get; }

    public BindableReactiveProperty<EditorControlViewModel?> ActiveDocument { get; }

    private readonly ObservableList<EditorControlViewModel> _editorViewModels = [];

    public NotifyCollectionChangedSynchronizedViewList<EditorControlViewModel> EditorViewModels { get; }

    private readonly SerialDisposable _activeDocumentIsDirtySubscription = new();

    private readonly Func<Model, EditorControlViewModel> _viewModelFactory;

    private readonly CompositeDisposable _disposable = [];

    public MainWindowViewModel(
        InteractionMessenger messenger,
        IStringLoader stringLoader,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(messenger);
        ArgumentNullException.ThrowIfNull(stringLoader);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this.InteractionMessenger = messenger;

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

        this.EditorViewModels = this._editorViewModels
            .ToNotifyCollectionChangedSlim()
            .AddTo(this._disposable);

        var factory = ActivatorUtilities.CreateFactory<EditorControlViewModel>([typeof(Model)]);

        this._viewModelFactory = model =>
        {
            var viewModel = factory(serviceProvider, [model]);

            var subscription = viewModel.ClosedEvent.Subscribe(
                (this, viewModel),
                static (_, args) =>
                {
                    var (self, vm) = ((MainWindowViewModel, EditorControlViewModel))args;

                    if (self._editorViewModels.Remove(vm))
                    {
                        vm.Dispose();
                    }
                });

            viewModel.RegisterDisposable(subscription);

            this._editorViewModels.Add(viewModel);

            return viewModel;
        };
    }

    private void OnNewDocument()
    {
        var viewModel = this._viewModelFactory(
            new NewFileModel(
                this._stringLoader.GetString("NewFileTitle")));
    }

    public async ValueTask OpenFilesAsync(CancellationToken cancellationToken)
    {
        var filePickRequest = new MultipleFileOpenPickerRequestMessage();
        var files = await this.InteractionMessenger.RequestMultipleFileOpenAsync(filePickRequest, cancellationToken);

        var modelFactory = new ModelFactory();

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var model = await modelFactory.OpenFileAsync(file, cancellationToken);

            var editorViewModel = this._viewModelFactory(model);

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

    public async ValueTask<bool> CloseAsync()
    {
        var dirtyTitles = this._editorViewModels
            .AsValueEnumerable()
            .Where(static x => x.IsDirty.Value)
            .Select(static x => x.Title.Value)
            .ToArray();

        var result = await this.InteractionMessenger.ConfirmSaveAsync(dirtyTitles);
        switch (result)
        {
            case SaveConfirmationResult.Save:
                // TODO: save
                return true;

            case SaveConfirmationResult.Discard:
                return true;

            case SaveConfirmationResult.Cancel:
                return false;
        }

        throw new UnreachableException();
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
