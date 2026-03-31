using Hexblick.Models;

using Microsoft.Extensions.DependencyInjection;

namespace Hexblick.ViewModels;

internal interface ITabItemViewModelFactory
{
    EditorControlViewModel Create(Model model);
}

internal sealed class TabItemViewModelFactory :
    ITabItemViewModelFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TabItemViewModelFactory(
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
