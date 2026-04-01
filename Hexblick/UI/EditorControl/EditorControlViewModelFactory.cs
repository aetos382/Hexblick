using System;

using Microsoft.Extensions.DependencyInjection;

using Hexblick.Models;

namespace Hexblick.UI;

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
