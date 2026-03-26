using Microsoft.Extensions.DependencyInjection;

namespace Hexblick;

internal interface ITabItemViewModelFactory
{
    TabItemViewModel Create();
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
    public TabItemViewModel Create()
    {
        return this._serviceProvider.GetRequiredService<TabItemViewModel>();
    }
}
