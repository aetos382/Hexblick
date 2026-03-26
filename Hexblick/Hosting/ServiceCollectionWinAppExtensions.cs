using Hexblick.Hosting;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

#pragma warning disable IDE0130
namespace Microsoft.Extensions.DependencyInjection;
#pragma warning restore IDE0130

internal static class ServiceCollectionWinAppExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection UseWinApp<TApp>()
            where TApp : Application
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<TApp>();
            services.Replace(ServiceDescriptor.Singleton<IHostLifetime, WinAppLifetime<TApp>>());

            return services;
        }
    }
}
