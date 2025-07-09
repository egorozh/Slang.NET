namespace Slang;
#pragma warning disable RS2000
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnexpectedErrorDescriptor = new("SLANG0001",
        "Unexpected error during generation",
        "Unexpected error occurred during code generation: {0}", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ServiceRequiredToConstructNotRegistered = new("SLANG0002",
        "The service registration not found",
        "The service '{0}' required to construct '{1}' is not registered", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MemberReferencedByInstanceOrFactoryAttributeNotFound = new("SLANG0003",
        "A member referenced by instance or factory attribute not found",
        "Unable to find a member '{0}', referenced by '{1}' attribute parameter", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MemberReferencedByInstanceOrFactoryAttributeAmbiguous = new("SLANG0004",
        "Found multiple members referenced by instance or factory attribute",
        "Found multiple members with the '{0}' name, referenced in the '{1}' attribute parameter", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ServiceProviderTypeHasToBePartial = new("SLANG0005",
        "The type marked with the ServiceProvider attribute has to be marked partial",
        "The type '{0}' marked with the ServiceProvider attribute has to be marked partial", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ImportedTypeNotMarkedWithModuleAttribute = new("SLANG0006",
        "The imported type has to be marked with the ServiceProviderModule attribute",
        "The imported type '{0}' has to be marked with the '{1}'", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ImplementationTypeRequiresPublicConstructor = new("SLANG0007",
        "The implementation type is required to have at least one public constructor",
        "The implementation type '{0}' is required to have at least one public constructor", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor CyclicDependencyDetected = new("SLANG0008",
        "A cyclic dependency detected when resolving a service",
        "A cyclic dependency detected when resolving a service '{0}', dependency chain: '{1}'", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor MissingServiceProviderAttribute = new("SLANG0009",
        "A type contains service registrations but no ServiceProvider or ServiceProviderModule attribute",
        "The type '{0}' contains service registration but is missing ServiceProvider or ServiceProviderModule attribute", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor NoServiceTypeRegistered = new("SLANG0010",
        "The service registration not found",
        "The service type '{0}' is not registered", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ImplementationTypeAndFactoryNotAllowed = new("SLANG0011",
        "Can't specify both the implementation type and factory/instance",
        "Can't specify both the implementation type and factory/instance, for service '{0}'", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor FactoryMemberMustBeAMethodOrHaveDelegateType = new("SLANG0012",
        "The factory member has to be a method or have a delegate type",
        "The factory member '{0}' has to be a method of have a delegate type, for service '{1}'", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ServiceNameMustBeAlphanumeric = new("SLANG0015",
        "Service name must be alphanumeric",
        "Service name '{0}' must be non-empty, alphanumeric and start with a letter.", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor BuiltInServicesAreNotNamed = new("SLANG0016",
        "Built-in provider services can not be named",
        "Built-in service '{0}' can not be named", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor ImplicitIEnumerableNotNamed = new("SLANG0017",
        "Implicit IEnumerable<> services can not be named",
        "Implicit IEnumerable service '{0}' can not be named", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor NoServiceTypeAndNameRegistered = new("SLANG0018",
        "The service registration not found",
        "The service type '{0}' and name '{1}' is not registered", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor NamedServiceRequiredToConstructNotRegistered = new("SLANG0019",
        "The named service registration not found",
        "The service '{0}' with name '{1}' required to construct '{2}' is not registered", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor OnlyStringKeysAreSupported = new("SLANG0020",
        "Only string service keys are supported",
        "Service key '{0}' is not supported, only string keys are supported", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor NullableServiceNotRegistered = new("SLANG0013",
        "Not registered nullable dependency without a default value",
        "The nullable service '{0}' requested to construct '{1}' is not registered. Add a default value to make the service reference optional", "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor NullableServiceRegistered = new("SLANG0014",
        "Nullable dependency without a default value",
        "'{0}' parameter to construct '{1}' will never be null when constructing using a service provider. Add a default value to make the service reference optional", "Usage", DiagnosticSeverity.Info, true);
}
