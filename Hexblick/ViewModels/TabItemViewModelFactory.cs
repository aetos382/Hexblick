using Microsoft.Extensions.DependencyInjection;

namespace Hexblick.ViewModels;

internal interface ITabItemViewModelFactory
{
    TabItemViewModel Create(bool isNewDocument = true);
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
    public TabItemViewModel Create(bool isNewDocument = true)
    {
        return ActivatorUtilities.CreateInstance<TabItemViewModel>(this._serviceProvider, isNewDocument);
    }
}
