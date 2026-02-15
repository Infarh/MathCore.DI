namespace MathCore.DI;

/// <summary>Атрибут, используемый для маркировки класса или интерфейса в качестве сервиса</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
public class ServiceAttribute : Attribute
{
    /// <summary>Получает или устанавливает режим временного lifetime сервиса</summary>
    public ServiceLifetime Mode { get; set; } = ServiceLifetime.Transient;

    /// <summary>Получает или устанавливает тип реализации сервиса</summary>
    /// <exception cref="InvalidOperationException">Вызывается, если тип реализации устанавливается после установки интерфейса.</exception>
    public Type? Implementation
    {
        get;
        set
        {
            if (Interface is not null)
                throw new InvalidOperationException(
                    "Нельзя устанавливать тип реализации после установки интерфейса. Реализацией сервиса должен быть класс.");
            field = value;
        }
    }

    /// <summary>Получает или устанавливает тип интерфейса сервиса</summary>
    /// <exception cref="InvalidOperationException">Вызывается, если тип интерфейса устанавливается после установки реализации.</exception>
    public Type? Interface
    {
        get;
        set
        {
            if (Implementation is not null)
                throw new InvalidOperationException(
                    "Нельзя устанавливать тип интерфейса после установки реализации. Интерфейсом должен быть тип.");
            field = value;
        }
    }

    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute"/></summary>
    public ServiceAttribute() { }

    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute"/> с указанным режимом временного lifetime</summary>
    /// <param name="Mode">Режим временного lifetime сервиса.</param>
    public ServiceAttribute(ServiceLifetime Mode) => this.Mode = Mode;

    /// <summary>Деконструирует атрибут сервиса в его части</summary>
    /// <param name="implementation">Тип реализации сервиса.</param>
    /// <param name="mode">Режим временного lifetime сервиса.</param>
    public void Deconstruct(out Type? implementation, out ServiceLifetime mode)
    {
        implementation = Implementation;
        mode = Mode;
    }

    /// <summary>Деконструирует атрибут сервиса в его части</summary>
    /// <param name="service">Тип интерфейса сервиса.</param>
    /// <param name="implementation">Тип реализации сервиса.</param>
    /// <param name="mode">Режим временного lifetime сервиса.</param>
    public void Deconstruct(out Type? service, out Type? implementation, out ServiceLifetime mode)
    {
        service = Interface;
        implementation = Implementation;
        mode = Mode;
    }
}