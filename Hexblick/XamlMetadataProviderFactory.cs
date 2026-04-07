using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

using ZLinq;

namespace Hexblick;

[RequiresDynamicCode("This class using reflection.")]
internal static class XamlMetadataProviderFactory
{
    public static IXamlMetadataProvider CreateProvider<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.NonPublicProperties)] TApplication>(
        TApplication app,
        string appProviderPropertyName)
        where TApplication : Application
    {
        var thisType = typeof(TApplication);

        var appProviderProperty = thisType.GetProperty(
            appProviderPropertyName,
            BindingFlags.Instance | BindingFlags.NonPublic);

        if (appProviderProperty is null)
        {
            throw new InvalidOperationException();
        }

        var appProviderMethods = appProviderProperty.PropertyType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .AsValueEnumerable();

        var thisParameter = Expression.Parameter(typeof(TApplication), "this");
        var appProviderPropertyExpression = Expression.Property(thisParameter, appProviderProperty);

        var typeNameParameter = Expression.Parameter(typeof(string), "typeName");
        var typeParameter = Expression.Parameter(typeof(Type), "type");

        var getXamlTypeByTypeName = Expression.Lambda<Func<TApplication, string, IXamlType>>(
            Expression.Call(
                appProviderPropertyExpression,
                appProviderMethods.Single(static x =>
                    x.Name == nameof(IXamlMetadataProvider.GetXamlType) &&
                    x.GetParameters()[0].ParameterType == typeof(string)),
                typeNameParameter),
            thisParameter, typeNameParameter);

        var getXamlTypeByType = Expression.Lambda<Func<TApplication, Type, IXamlType>>(
            Expression.Call(
                appProviderPropertyExpression,
                appProviderMethods.Single(static x =>
                    x.Name == nameof(IXamlMetadataProvider.GetXamlType) &&
                    x.GetParameters()[0].ParameterType == typeof(Type)),
                typeParameter),
            thisParameter, typeParameter);

        var getXmlnsDefinitions = Expression.Lambda<Func<TApplication, XmlnsDefinition[]>>(
            Expression.Call(
                appProviderPropertyExpression,
                appProviderMethods.Single(static x =>
                    x.Name == nameof(IXamlMetadataProvider.GetXmlnsDefinitions))),
            thisParameter);

        return new DelegateClass<TApplication>(
            app,
            getXamlTypeByTypeName.Compile(),
            getXamlTypeByType.Compile(),
            getXmlnsDefinitions.Compile());
    }

    private sealed class DelegateClass<TApplication> : IXamlMetadataProvider
        where TApplication : Application
    {
        private readonly TApplication _instance;
        private readonly Func<TApplication, string, IXamlType> _getXamlTypeByString;
        private readonly Func<TApplication, Type, IXamlType> _getXamlTypeByType;
        private readonly Func<TApplication, XmlnsDefinition[]> _getXmlDefinitions;

        public DelegateClass(
            TApplication instance,
            Func<TApplication, string, IXamlType> getXamlTypeByString,
            Func<TApplication, Type, IXamlType> getXamlTypeByType,
            Func<TApplication, XmlnsDefinition[]> getXmlDefinitions)
        {
            this._instance = instance;
            this._getXamlTypeByString = getXamlTypeByString;
            this._getXamlTypeByType = getXamlTypeByType;
            this._getXmlDefinitions = getXmlDefinitions;
        }

        IXamlType IXamlMetadataProvider.GetXamlType(Type type)
        {
            return this._getXamlTypeByType(this._instance, type);
        }

        IXamlType IXamlMetadataProvider.GetXamlType(string fullName)
        {
            return this._getXamlTypeByString(this._instance, fullName);
        }

        XmlnsDefinition[] IXamlMetadataProvider.GetXmlnsDefinitions()
        {
            return this._getXmlDefinitions(this._instance);
        }
    }
}
