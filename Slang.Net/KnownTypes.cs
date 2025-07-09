namespace Slang;

internal class KnownTypes
{
    public const string JabAttributesAssemblyName = "Slang.Attributes";

    public const string TranslationsAttributeShortName = "Translations";
    public const string TranslationsAttributeTypeName = $"{TranslationsAttributeShortName}Attribute";

    private const string IEnumerableMetadataName = "System.Collections.Generic.IEnumerable`1";
    private const string IServiceProviderMetadataName = "System.IServiceProvider";

    private const string IKeyedServiceProviderMetadataName =
        "Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider";

    private const string FromKeyedServicesAttributeMetadataName =
        "Microsoft.Extensions.DependencyInjection.FromKeyedServicesAttribute";

    private const string IServiceScopeFactoryMetadataName =
        "Microsoft.Extensions.DependencyInjection.IServiceScopeFactory";

    private const string IServiceProviderIsServiceMetadataName =
        "Microsoft.Extensions.DependencyInjection.IServiceProviderIsService";

    public INamedTypeSymbol IEnumerableType { get; }

    public INamedTypeSymbol IServiceProviderType { get; }

    public INamedTypeSymbol? IServiceScopeFactoryType { get; }
    public INamedTypeSymbol? IServiceProviderIsServiceType { get; }
    public INamedTypeSymbol? FromKeyedServicesAttribute { get; }

    public KnownTypes(Compilation compilation)
    {
        static INamedTypeSymbol GetTypeFromCompilationByMetadataNameOrThrow(Compilation compilation,
            string fullyQualifiedMetadataName) =>
            compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)
            ?? throw new InvalidOperationException($"Type with metadata '{fullyQualifiedMetadataName}' not found");

        IEnumerableType = GetTypeFromCompilationByMetadataNameOrThrow(compilation, IEnumerableMetadataName);
        IServiceProviderType = GetTypeFromCompilationByMetadataNameOrThrow(compilation, IServiceProviderMetadataName);
        IServiceScopeFactoryType = compilation.GetTypeByMetadataName(IServiceScopeFactoryMetadataName);
        IServiceProviderIsServiceType = compilation.GetTypeByMetadataName(IServiceProviderIsServiceMetadataName);
        FromKeyedServicesAttribute = compilation.GetTypeByMetadataName(FromKeyedServicesAttributeMetadataName);
    }
}