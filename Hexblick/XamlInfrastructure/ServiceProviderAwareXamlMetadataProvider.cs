using System;
using System.Collections.Generic;

using Microsoft.UI.Xaml.Markup;

namespace Hexblick.XamlInfrastructure;

internal class ServiceProviderAwareXamlMetadataProvider :
    IXamlMetadataProvider
{
    private readonly IXamlMetadataProvider _coreProvider;
    private readonly IServiceProvider _serviceProvider;

    public ServiceProviderAwareXamlMetadataProvider(
        IXamlMetadataProvider coreProvider,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(coreProvider);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._coreProvider = coreProvider;
        this._serviceProvider = serviceProvider;
    }

    private readonly Dictionary<string, IXamlType> _typeCacheByName = new(StringComparer.Ordinal);
    private readonly Dictionary<Type, IXamlType> _typeCacheByType = new();

    /// <inheritdoc />
    IXamlType IXamlMetadataProvider.GetXamlType(Type type)
    {
        if (!this._typeCacheByType.TryGetValue(type, out var xamlType))
        {
            this._typeCacheByType[type] = xamlType = this.GetXamlTypeCore(type);
        }

        return xamlType;
    }

    /// <inheritdoc />
    IXamlType IXamlMetadataProvider.GetXamlType(string fullName)
    {
        if (!this._typeCacheByName.TryGetValue(fullName, out var xamlType))
        {
            this._typeCacheByName[fullName] = xamlType = this.GetXamlTypeCore(fullName);
        }

        return xamlType;
    }

    /// <inheritdoc />
    XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
    {
        return this._coreProvider.GetXmlnsDefinitions();
    }

    private IXamlType GetXamlTypeCore(Type type)
    {
        var coreType = this._coreProvider.GetXamlType(type);
        if (coreType is null)
        {
            return null;
        }

        return new ServiceProviderAwareXamlType(coreType, this._serviceProvider);
    }

    private IXamlType GetXamlTypeCore(string fullName)
    {
        var coreType = this._coreProvider.GetXamlType(fullName);
        if (coreType is null)
        {
            return null;
        }

        return new ServiceProviderAwareXamlType(coreType, this._serviceProvider);
    }
}
