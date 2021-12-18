using System.Diagnostics.CodeAnalysis;

namespace MathCore.DI.Tests;

[TestClass]
public class ComplexInjectionRegistratorTests
{
    private class TestClass
    {
        private readonly string _Name;
        private readonly string? _Value;

        public TestClass(string Name, string? Value)
        {
            _Name = Name;
            _Value = Value;
        }
    }

    private interface ITestService1 { }

    private class SimpleTestService1 : ITestService1 { }

    [TestMethod]
    public void SimpleRegistration()
    {
        var service_collection = new ServiceCollection();

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService1);
        var implementation_type = typeof(SimpleTestService1);
        service_collection.AddService(service_type, implementation_type, service_lifetime);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(1);
        Assert.That.Value(descriptors[0])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNull("Неверно был сформирован фабричный метод экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsEqual(implementation_type));

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        Assert.That.Value(instance).Is(implementation_type);
    }

    private interface ITestService2 { }

    private class Service2WithPrivateFieldOfService1 : ITestService2
    {
        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Добавить модификатор только для чтения")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private ITestService1 _Service1 = null!;

        public ITestService1 GetFieldValue() => _Service1;
    }

    [TestMethod]
    public void InjectionWithFieldsPrivate()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        service_collection.AddSingleton<ITestService1>(service1_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithPrivateFieldOfService1);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithPrivateFieldOfService1>();

        var field_value = service2_instance.ActualValue.GetFieldValue();

        Assert.That.Value(field_value).IsEqual(service1_instance);
    }

    private class Service2WithPrivateReadonlyFieldOfService1 : ITestService2
    {
        [Inject]
        private readonly ITestService1 _Service1 = null!;

        public ITestService1 GetFieldValue() => _Service1;
    }

    [TestMethod]
    public void InjectionWithFieldsPrivateReadonly()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        service_collection.AddSingleton<ITestService1>(service1_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithPrivateReadonlyFieldOfService1);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithPrivateReadonlyFieldOfService1>();

        var field_value = service2_instance.ActualValue.GetFieldValue();

        Assert.That.Value(field_value).IsEqual(service1_instance);
    }

    private class Service2WithPrivatePropertyOfService1 : ITestService2
    {
        [Inject]
        private ITestService1 Service1 { get; set; } = null!;

        public ITestService1 GetPropertyValue() => Service1;
    }

    [TestMethod]
    public void InjectionWithPropertyPrivate()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        service_collection.AddSingleton<ITestService1>(service1_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithPrivatePropertyOfService1);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithPrivatePropertyOfService1>();

        var field_value = service2_instance.ActualValue.GetPropertyValue();

        Assert.That.Value(field_value).IsEqual(service1_instance);
    }

    private class Service2WithPrivateInitPropertyOfService1 : ITestService2
    {
        [Inject]
        private ITestService1 Service1 { get; init; } = null!;

        public ITestService1 GetPropertyValue() => Service1;
    }

    [TestMethod]
    public void InjectionWithPropertyInitPrivate()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        service_collection.AddSingleton<ITestService1>(service1_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithPrivateInitPropertyOfService1);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithPrivateInitPropertyOfService1>();

        var field_value = service2_instance.ActualValue.GetPropertyValue();

        Assert.That.Value(field_value).IsEqual(service1_instance);
    }

    private interface ITestService3 { }

    private class SimpleTestService3 : ITestService3 { }

    private class Service2WithConstructorParameterOfService1AndFieldOfService3 : ITestService2
    {
        private readonly ITestService1 _Service1;

        [Inject]
        private readonly ITestService3 _Service3 = null!;

        public Service2WithConstructorParameterOfService1AndFieldOfService3(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1 GetService1Value() => _Service1;
        public ITestService3 GetService3Value() => _Service3;
    }

    [TestMethod]
    public void InjectionWithConstructorAndPropertyParameter()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        var service3_instance = new SimpleTestService3();
        service_collection.AddSingleton<ITestService1>(service1_instance);
        service_collection.AddSingleton<ITestService3>(service3_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithConstructorParameterOfService1AndFieldOfService3);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(3);
        Assert.That.Value(descriptors[2])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithConstructorParameterOfService1AndFieldOfService3>();

        var field_value_service1 = service2_instance.ActualValue.GetService1Value();
        var field_value_service3 = service2_instance.ActualValue.GetService3Value();

        Assert.That.Value(field_value_service1).IsEqual(service1_instance);
        Assert.That.Value(field_value_service3).IsEqual(service3_instance);
    }

    private class Service2WithConstructorParameterInjectAttribute : ITestService2
    {
        private ITestService1 _Service1;

        public Service2WithConstructorParameterInjectAttribute([Inject] ITestService1 Service1) => _Service1 = Service1;

        public ITestService1 GetService1Value() => _Service1;
    }

    [TestMethod]
    public void InjectionWithConstructorParameterInjectAttribute()
    {
        var service_collection = new ServiceCollection();

        var service1_instance = new SimpleTestService1();
        service_collection.AddSingleton<ITestService1>(service1_instance);

        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;
        var service_type = typeof(ITestService2);
        var implementation_type = typeof(Service2WithConstructorParameterInjectAttribute);
        service_collection.AddService(service_type, implementation_type, ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_type))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var instance = provider.GetService(service_type);
        var service2_instance = Assert.That.Value(instance).As<Service2WithConstructorParameterInjectAttribute>();

        var field_value_service1 = service2_instance.ActualValue.GetService1Value();

        Assert.That.Value(field_value_service1).IsEqual(service1_instance);
    }
}