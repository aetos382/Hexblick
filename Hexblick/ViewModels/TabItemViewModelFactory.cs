using Hexblick.Models;

using Microsoft.Extensions.DependencyInjection;

namespace Hexblick.ViewModels;

internal interface ITabItemViewModelFactory
{
    TabItemViewModel Create(Model model);
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
    public TabItemViewModel Create(Model model)
    {
        ArgumentNullException.ThrowIfNull(model);

        return ActivatorUtilities.CreateInstance<TabItemViewModel>(this._serviceProvider, model);
    }
}
