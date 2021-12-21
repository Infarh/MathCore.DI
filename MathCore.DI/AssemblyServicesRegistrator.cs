namespace MathCore.DI;

/// <summary>Средства для массовой регистрации сервисов из указанной сборки</summary>
public static class AssemblyServicesRegistrator
{
    /// <summary>Зарегистрировать все сервисы из указанной сборки</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="assembly">Сборка, содержащая сервисы</param>
    /// <param name="OnServiceAdded">Метод обработки зарегистрированного типа сервиса</param>
    /// <returns>Коллекция сервисов с зарегистрированными в ней сервисами из указанной сборки</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly, Action<Type, Type?, ServiceLifetime>? OnServiceAdded)
    {
        foreach (var type in assembly.DefinedTypes)
        {
            var info = type.GetCustomAttribute<ServiceAttribute>();
            if (info is null) continue;
            services.AddService(type, info.Implementation, info.Mode);
            OnServiceAdded?.Invoke(type, info.Implementation, info.Mode);
        }

        return services;
    }

    /// <summary>Зарегистрировать все сервисы из указанной сборки</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="assembly">Сборка, содержащая сервисы</param>
    /// <returns>Коллекция сервисов с зарегистрированными в ней сервисами из указанной сборки</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Assembly assembly) =>
        services.AddServicesFromAssembly(assembly, null);

    /// <summary>Зарегистрировать все сервисы из сборки, содержащей указанный тип</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="AssemblyType">Тип, определяющий сборку для поиска сервисов</param>
    /// <param name="OnServiceAdded">Метод обработки зарегистрированного типа сервиса</param>
    /// <returns>Коллекция сервисов с зарегистрированными в ней сервисами из указанной сборки</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Type AssemblyType, Action<Type, Type?, ServiceLifetime>? OnServiceAdded) =>
        services.AddServicesFromAssembly(AssemblyType.Assembly, OnServiceAdded);

    /// <summary>Зарегистрировать все сервисы из сборки, содержащей указанный тип</summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="AssemblyType">Тип, определяющий сборку для поиска сервисов</param>
    /// <returns>Коллекция сервисов с зарегистрированными в ней сервисами из указанной сборки</returns>
    public static IServiceCollection AddServicesFromAssembly(this IServiceCollection services, Type AssemblyType) =>
        services.AddServicesFromAssembly(AssemblyType.Assembly);
}