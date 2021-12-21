namespace MathCore.DI;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class InjectAttribute : Attribute
{
    public bool Required { get; set; } = true;

    public InjectAttribute() { }

    public InjectAttribute(bool Required) => this.Required = Required;
}