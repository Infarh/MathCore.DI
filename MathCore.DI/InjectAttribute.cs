namespace MathCore.DI;

/// <summary>Атрибут, указывающий на необходимость внедрения зависимости</summary>
/// <remarks>Инициализирует новый экземпляр класса <see cref="InjectAttribute"/> с указанной обязательностью внедрения</remarks>
/// <param name="Required">Обязательность внедрения зависимости.</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Parameter)]
public sealed class InjectAttribute(bool Required) : Attribute
{
    /// <summary>Обязательность внедрения зависимости</summary>
    public bool Required { get; set; } = Required;

    /// <summary>Инициализирует новый экземпляр класса <see cref="InjectAttribute"/></summary>
    public InjectAttribute() : this(true) { }
}