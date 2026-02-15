namespace MathCore.DI;

/// <summary>Атрибут сервиса, позволяющий указать тип сервиса и его реализацию</summary>
/// <typeparam name="TService">Тип сервиса</typeparam>
public class ServiceAttribute<TService> : ServiceAttribute
    where TService : class
{
    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute{TService}"/></summary>
    public ServiceAttribute() => SetType(typeof(TService));

    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute{TService}"/> с указанным режимом временного cycle lifetime</summary>
    /// <param name="Mode">Режим временного cycle lifetime сервиса</param>
    public ServiceAttribute(ServiceLifetime Mode) : base(Mode) => SetType(typeof(TService));

    /// <summary>Устанавливает тип сервиса и его реализацию</summary>
    /// <param name="ServiceType">Тип сервиса</param>
    private void SetType(Type ServiceType)
    {
        if (ServiceType.IsInterface)
            Interface = ServiceType;
        else
            Implementation = ServiceType;
    }
}

/// <summary>Атрибут, используемый для маркировки класса или интерфейса в качестве сервиса</summary>
/// <typeparam name="TInterface">Тип интерфейса сервиса</typeparam>
/// <typeparam name="TService">Тип реализации сервиса</typeparam>
public class ServiceAttribute<TInterface, TService> : ServiceAttribute
    where TInterface : class
    where TService : class, TInterface
{
    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute{TInterface, TService}"/></summary>
    public ServiceAttribute() => SetType(typeof(TInterface), typeof(TService));

    /// <summary>Инициализирует новый экземпляр класса <see cref="ServiceAttribute{TInterface, TService}"/> с указанным режимом временного cycle lifetime</summary>
    /// <param name="Mode">Режим временного cycle lifetime сервиса</param>
    public ServiceAttribute(ServiceLifetime Mode) : base(Mode) => SetType(typeof(TInterface), typeof(TService));

    /// <summary>Устанавливает тип интерфейса и реализации сервиса</summary>
    /// <param name="InterfaceType">Тип интерфейса сервиса</param>
    /// <param name="ServiceType">Тип реализации сервиса</param>
    private void SetType(Type InterfaceType, Type ServiceType)
    {
        Interface = InterfaceType;
        Implementation = ServiceType;
    }
}
