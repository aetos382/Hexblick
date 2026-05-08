using System;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using Hexblick.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

internal static class WindowsAppServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection UseWinApp<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TApplication>()
            where TApplication : Application
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<Application, TApplication>();
            services.Replace(ServiceDescriptor.Singleton<IHostLifetime, WindowsAppLifetime>());

            return services;
        }
    }
}
