using System;

using Microsoft.Extensions.DependencyInjection;

using Hexblick.Models;

namespace Hexblick.Presentations;

internal interface IDocumentManager
{
    EditorControlViewModel CreateDocument(
        Model model);
}

internal sealed class DocumentManager :
    IDocumentManager,
    IDisposable
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly Func<IServiceProvider, Model, EditorControlViewModel> _viewModelFactory;

    public DocumentManager(
        IServiceScopeFactory serviceScopeFactory)
    {
        ArgumentNullException.ThrowIfNull(serviceScopeFactory);

        this._serviceScopeFactory = serviceScopeFactory;

        var factory = ActivatorUtilities.CreateFactory<EditorControlViewModel>([typeof(Model)]);
        this._viewModelFactory = (sp, model) => factory(sp, [model]);
    }

    public EditorControlViewModel CreateDocument(
        Model model)
    {
        var scope = this._serviceScopeFactory.CreateScope();
        var vm = this._viewModelFactory(scope.ServiceProvider, model);

        return vm;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
