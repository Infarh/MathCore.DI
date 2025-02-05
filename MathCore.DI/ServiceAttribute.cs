namespace MathCore.DI;

/// <summary>
/// Атрибут, используемый для маркировки класса или интерфейса в качестве сервиса.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false, AllowMultiple = false)]
public class ServiceAttribute : Attribute
{
    /// <summary>
    /// Получает или устанавливает режим временного lifetime сервиса.
    /// </summary>
    public ServiceLifetime Mode { get; set; } = ServiceLifetime.Transient;

    private Type? _Implementation;

    /// <summary>
    /// Получает или устанавливает тип реализации сервиса.
    /// </summary>
    /// <exception cref="InvalidOperationException">Вызывается, если тип реализации устанавливается после установки интерфейса.</exception>
    public Type? Implementation
    {
        get => _Implementation;
        set
        {
            if (_Interface is not null)
                throw new InvalidOperationException("Нельзя устанавливать тип реализации после установки интерфейса. Реализацией сервиса должен быть класс.");
            _Implementation = value;
        }
    }

    private Type? _Interface;

    /// <summary>
    /// Получает или устанавливает тип интерфейса сервиса.
    /// </summary>
    /// <exception cref="InvalidOperationException">Вызывается, если тип интерфейса устанавливается после установки реализации.</exception>
    public Type? Interface
    {
        get => _Interface;
        set
        {
            if (_Implementation is not null)
                throw new InvalidOperationException("Нельзя устанавливать тип интерфейса после установки реализации. Интерфейсом должен быть тип.");
            _Interface = value;
        }
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ServiceAttribute"/>.
    /// </summary>
    public ServiceAttribute() { }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ServiceAttribute"/> с указанным режимом временного lifetime.
    /// </summary>
    /// <param name="Mode">Режим временного lifetime сервиса.</param>
    public ServiceAttribute(ServiceLifetime Mode) => this.Mode = Mode;

    /// <summary>
    /// Деконструирует атрибут сервиса в его части.
    /// </summary>
    /// <param name="Implementation">Тип реализации сервиса.</param>
    /// <param name="Mode">Режим временного lifetime сервиса.</param>
    public void Deconstruct(out Type? Implementation, out ServiceLifetime Mode)
    {
        Implementation = this.Implementation;
        Mode = this.Mode;
    }

    /// <summary>
    /// Деконструирует атрибут сервиса в его части.
    /// </summary>
    /// <param name="Service">Тип интерфейса сервиса.</param>
    /// <param name="Implementation">Тип реализации сервиса.</param>
    /// <param name="Mode">Режим временного lifetime сервиса.</param>
    public void Deconstruct(out Type? Service, out Type? Implementation, out ServiceLifetime Mode)
    {
        Service = _Interface;
        Implementation = _Implementation;
        Mode = this.Mode;
    }
}