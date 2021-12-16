using Microsoft.Extensions.DependencyInjection;

namespace MathCore.DI;

public static class Compositor
{
    public static IServiceCollection AddComposite<TInterface>(this IServiceCollection services, ServiceLifetime ServiceLifetime = ServiceLifetime.Transient)
    {
        var interface_type = typeof(TInterface);

        services.Add(new ServiceDescriptor(interface_type, interface_type.CreateImplementation(), ServiceLifetime));

        return services;
    }
}

