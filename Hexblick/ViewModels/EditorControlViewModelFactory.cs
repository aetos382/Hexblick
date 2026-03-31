using Hexblick.Models;

using Microsoft.Extensions.DependencyInjection;

namespace Hexblick.ViewModels;

internal interface IEditorControlViewModelFactory
{
    EditorControlViewModel Create(Model model);
}

internal sealed class EditorControlViewModelFactory :
    IEditorControlViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public EditorControlViewModelFactory(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public EditorControlViewModel Create(Model model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return ActivatorUtilities.CreateInstance<EditorControlViewModel>(this._serviceProvider, model);
    }
}
