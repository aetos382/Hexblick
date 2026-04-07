using System;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Markup;

namespace Hexblick.XamlInfrastructure;

internal sealed class ServiceProviderAwareXamlType :
    IXamlType
{
    private readonly IXamlType _coreType;
    private readonly IServiceProvider _serviceProvider;
    private readonly IServiceProviderIsService _serviceProviderIsService;

    public ServiceProviderAwareXamlType(
        IXamlType coreType,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(coreType);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._coreType = coreType;
        this._serviceProvider = serviceProvider;
        this._serviceProviderIsService = serviceProvider.GetRequiredService<IServiceProviderIsService>();
    }

    /// <inheritdoc />
    object IXamlType.ActivateInstance()
    {
        var underlyingType = this._coreType.UnderlyingType;

        if (this._serviceProviderIsService.IsService(underlyingType))
        {
            return this._serviceProvider.GetRequiredService(underlyingType);
        }
        else
        {
            return this._coreType.ActivateInstance();
        }
    }

    /// <inheritdoc />
    object IXamlType.CreateFromString(string value)
    {
        return this._coreType.CreateFromString(value);
    }

    /// <inheritdoc />
    IXamlMember IXamlType.GetMember(string name)
    {
        return this._coreType.GetMember(name);
    }

    /// <inheritdoc />
    void IXamlType.AddToVector(object instance, object value)
    {
        this._coreType.AddToVector(instance, value);
    }

    /// <inheritdoc />
    void IXamlType.AddToMap(object instance, object key, object value)
    {
        this._coreType.AddToMap(instance, key, value);
    }

    /// <inheritdoc />
    void IXamlType.RunInitializer()
    {
        this._coreType.RunInitializer();
    }

    /// <inheritdoc />
    IXamlType IXamlType.BaseType => this._coreType.BaseType;

    /// <inheritdoc />
    IXamlType IXamlType.BoxedType => this._coreType.BoxedType;

    /// <inheritdoc />
    IXamlMember IXamlType.ContentProperty => this._coreType.ContentProperty;

    /// <inheritdoc />
    string IXamlType.FullName => this._coreType.FullName;

    /// <inheritdoc />
    bool IXamlType.IsArray => this._coreType.IsArray;

    /// <inheritdoc />
    bool IXamlType.IsBindable => this._coreType.IsBindable;

    /// <inheritdoc />
    bool IXamlType.IsCollection => this._coreType.IsCollection;

    /// <inheritdoc />
    bool IXamlType.IsConstructible => this._coreType.IsConstructible;

    /// <inheritdoc />
    bool IXamlType.IsDictionary => this._coreType.IsDictionary;

    /// <inheritdoc />
    bool IXamlType.IsMarkupExtension => this._coreType.IsMarkupExtension;

    /// <inheritdoc />
    IXamlType IXamlType.ItemType => this._coreType.ItemType;

    /// <inheritdoc />
    IXamlType IXamlType.KeyType => this._coreType.KeyType;

    /// <inheritdoc />
    Type IXamlType.UnderlyingType => this._coreType.UnderlyingType;
}
