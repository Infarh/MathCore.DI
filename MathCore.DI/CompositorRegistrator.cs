namespace MathCore.DI;

public static class CompositorRegistrator
{
    /// <summary>Добавить композицию сервисов</summary>
    /// <typeparam name="TInterface">Интерфейс-композиция сервисов, содержащий набор свойств для внедрения зависимостей</typeparam>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="ServiceLifetime">Режим регистрации</param>
    /// <returns>Коллекция сервисов, с зарегистрированным в ней сервисом-композитором других сервисов</returns>
    /// <exception cref="InvalidOperationException">В случае наличия в интерфейсе методов, событий, индексаторов</exception>
    public static IServiceCollection AddComposite<TInterface>(this IServiceCollection services, ServiceLifetime ServiceLifetime = ServiceLifetime.Transient)
    {
        var interface_type = typeof(TInterface);

        const BindingFlags instance = BindingFlags.Public | BindingFlags.Instance;
        var methods = interface_type.GetMethods(instance);
        if (methods.Any(method => !method.Name.StartsWith("get_") && !method.Name.StartsWith("set_")))
            throw new InvalidOperationException("В указанном интерфейсе присутствуют методы. Интерфейс может содержать только свойства.")
            {
                Data =
                {
                    { "Methods", string.Join("; ", methods.Select(m => $"{m.ReturnType} {m.Name}({(string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType} {p.Name}")))})")) }
                }
            };

        if (interface_type.GetEvents(instance) is { Length: > 0 } events)
            throw new InvalidOperationException("В указанном интерфейсе присутствуют события. Интерфейс может содержать только свойства.")
            {
                Data =
                {
                    { "Events", string.Join(", ", events.Select(e => $"{e.EventHandlerType} {e.Name}")) }
                }
            };

        if (interface_type.GetProperties(instance).Any(p => p.GetIndexParameters() is { Length: > 0 }))
            throw new InvalidOperationException("В указанном интерфейсе присутствуют индексаторы. Интерфейс может содержать только свойства.");

        services.Add(new ServiceDescriptor(interface_type, interface_type.CreateImplementation(), ServiceLifetime));

        return services;
    }
}

