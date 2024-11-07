using System;
using System.Collections.Generic;
using System.Text;

namespace MathCore.DI;

public class ServiceAttribute<TService> : ServiceAttribute
    where TService : class
{
    public ServiceAttribute() => SetType(typeof(TService));

    public ServiceAttribute(ServiceLifetime Mode) : base(Mode) => SetType(typeof(TService));

    private void SetType(Type ServiceType)
    {
        if (ServiceType.IsInterface)
            Interface = ServiceType;
        else
            Implementation = ServiceType;
    }
}

public class ServiceAttribute<TInterface, TService> : ServiceAttribute 
    where TInterface : class
    where TService : class, TInterface
{
    public ServiceAttribute() => SetType(typeof(TInterface), typeof(TService));

    public ServiceAttribute(ServiceLifetime Mode) : base(Mode) => SetType(typeof(TInterface), typeof(TService));

    private void SetType(Type InterfaceType, Type ServiceType)
    {
        Interface = InterfaceType;
        Implementation = ServiceType;
    }
}
