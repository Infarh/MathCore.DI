#if !NET7_0_OR_GREATER

// ReSharper disable once CheckNamespace
namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that the output will be non-null if the named parameter is non-null.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple = true, Inherited = false)]
public sealed class NotNullIfNotNullAttribute : Attribute
{
    /// <summary>Initializes the attribute with the associated parameter name.</summary>
    /// <param name="parameterName">The associated parameter name.  The output will be non-null if the argument to the parameter specified is non-null.</param>
    public NotNullIfNotNullAttribute(string parameterName) => this.ParameterName = parameterName;

    /// <summary>Gets the associated parameter name.</summary>
    /// <returns>The associated parameter name. The output will be non-null if the argument to the parameter specified is non-null.</returns>
    public string ParameterName { get; }
}

/// <summary>Specifies that an output is not <see langword="null" /> even if the corresponding type allows it. Specifies that an input argument was not <see langword="null" /> when the call returns.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
public sealed class NotNullAttribute : Attribute;

#endif