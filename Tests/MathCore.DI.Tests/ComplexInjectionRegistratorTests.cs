using MathCore.DI.Tests.Interfaces;

namespace MathCore.DI.Tests;

[TestClass]
public class ComplexInjectionRegistratorTests
{
    /// <summary>Простая реализация тестового сервиса - пустой класс</summary>
    private class SimpleTestService1 : ITestService1 { }

    /// <summary>Проверка регистрации сервиса, подразумевающая стандартное поведение контейнера</summary>
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

    /// <summary>Второй тестовый сервис, реализующий внедрение через поле</summary>
    private class Service2WithPrivateFieldOfService1 : ITestService2
    {
        /// <summary>Поле, в которое должно произойти внедрение экземпляра сервиса</summary>
        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Добавить модификатор только для чтения")]
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private ITestService1 _Service1 = null!;
        
        /// <summary>Метод, предназначенный для извлечения значения поля для проверки факта внедрения</summary>
        /// <returns>Значение поля <see cref="_Service1"/></returns>
        public ITestService1 GetFieldValue() => _Service1;
    }

    /// <summary>Проверка возможности осуществления внедрения сервиса в приватное поле, помеченное атрибутом <see cref="InjectAttribute"/></summary>
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

    /// <summary>Тестовая реализация сервиса со скрытым полем тольео для чтения, кда требуется выполнить внедрение свойства</summary>
    private class Service2WithPrivateReadonlyFieldOfService1 : ITestService2
    {
        [Inject]
        private readonly ITestService1 _Service1 = null!;

        public ITestService1 GetFieldValue() => _Service1;
    }

    /// <summary>Тест внедрения в <c>private readonly</c> поле</summary>
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

    /// <summary>Тестовый метод с приватным свойством для чтения и для записи</summary>
    private class Service2WithPrivatePropertyOfService1 : ITestService2
    {
        [Inject]
        private ITestService1 Service1 { get; set; } = null!;

        public ITestService1 GetPropertyValue() => Service1;
    }

    /// <summary>Тестирование внедрения в приватное свойство с доступом для чтения и для записи</summary>
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

    /// <summary>Тестовый сервис с приватным свойством с доступом для чтения и для инициализации</summary>
    private class Service2WithPrivateInitPropertyOfService1 : ITestService2
    {
        [Inject]
        private ITestService1 Service1 { get; init; } = null!;

        public ITestService1 GetPropertyValue() => Service1;
    }

    /// <summary>Тестирование внедрения в приватное свойство с доступом для чтения и инициализации</summary>
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

    /// <summary>Простая реализация тестового сервиса 3</summary>
    private class SimpleTestService3 : ITestService3 { }

    /// <summary>Реализация сервиса 2 с внедрением в приватное поле и конструктор</summary>
    private class Service2WithConstructorParameterOfService1AndFieldOfService3 : ITestService2
    {
        private readonly ITestService1 _Service1;

        [Inject]
        private readonly ITestService3 _Service3 = null!;

        public Service2WithConstructorParameterOfService1AndFieldOfService3(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1 GetService1Value() => _Service1;
        public ITestService3 GetService3Value() => _Service3;
    }

    /// <summary>Тестирование внедрение в приватное поле и конструктор одновременно</summary>
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

    /// <summary>Тестовая реализация сервиса с выполнением внедрения в конструктор с управленем атрибутом <see cref="InjectAttribute"/></summary>
    private class Service2WithConstructorParameterInjectAttribute : ITestService2
    {
        private readonly ITestService1 _Service1;

        public Service2WithConstructorParameterInjectAttribute([Inject] ITestService1 Service1) => _Service1 = Service1;

        public ITestService1 GetService1Value() => _Service1;
    }

    /// <summary>Тестирование управляемого внедрения в конструктор с помощью атрибута <see cref="InjectAttribute"/></summary>
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

    /// <summary>Тестовая реализация сервиса 2 с опциональным параметром в конструкторе</summary>
    private class Service2WithOptionalConstructorParameter : ITestService2
    {
        private readonly ITestService1 _Service1;

        public Service2WithOptionalConstructorParameter([Inject(false)] ITestService1 Service1) => _Service1 = Service1;

        public ITestService1 GetService1() => _Service1;
    }

    /// <summary>Тестирование сервиса с опциональным параметром в конструкторе</summary>
    [TestMethod]
    public void InjectWithOptionalConstructorParameter()
    {
        var service_collection_with_service1 = new ServiceCollection();
        var service_collection_without_service1 = new ServiceCollection();

        service_collection_with_service1.AddSingleton<ITestService1>(new SimpleTestService1());

        var service1_interface = typeof(ITestService2);
        var service2_implementation = typeof(Service2WithOptionalConstructorParameter);
        service_collection_with_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);
        service_collection_without_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);

        var services_with_service1 = service_collection_with_service1.BuildServiceProvider();
        var services_without_service1 = service_collection_without_service1.BuildServiceProvider();

        var service1_1 = services_with_service1.GetService<ITestService1>();
        var service1_2 = services_without_service1.GetService<ITestService1>();

        Assert.That.Value(service1_1).IsNotNull();
        Assert.That.Value(service1_2).IsNull();

        var service2_1 = services_with_service1.GetRequiredService<ITestService2>();
        var service2_2 = services_without_service1.GetRequiredService<ITestService2>();

        Assert.That.Value(service2_1)
           .As<Service2WithOptionalConstructorParameter>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNotNull());

        Assert.That.Value(service2_2)
           .As<Service2WithOptionalConstructorParameter>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNull());
    }

    /// <summary>Тестовая реализация сервиса 2 с опциональным полем</summary>
    private class Service2WithOptionalPrivateReadonlyField : ITestService2
    {
        [Inject(false)]
        private readonly ITestService1? _Service1;

        public ITestService1? GetService1() => _Service1;
    }

    /// <summary>Тестирование возможности внедрения сервиса в опциональное поле при отсутствии сервиса в контейнере</summary>
    [TestMethod]
    public void InjectWithOptionalField()
    {
        var service_collection_with_service1 = new ServiceCollection();
        var service_collection_without_service1 = new ServiceCollection();

        service_collection_with_service1.AddSingleton<ITestService1>(new SimpleTestService1());

        var service1_interface = typeof(ITestService2);
        var service2_implementation = typeof(Service2WithOptionalPrivateReadonlyField);
        service_collection_with_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);
        service_collection_without_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);

        var services_with_service1 = service_collection_with_service1.BuildServiceProvider();
        var services_without_service1 = service_collection_without_service1.BuildServiceProvider();

        var service1_1 = services_with_service1.GetService<ITestService1>();
        var service1_2 = services_without_service1.GetService<ITestService1>();

        Assert.That.Value(service1_1).IsNotNull();
        Assert.That.Value(service1_2).IsNull();

        var service2_1 = services_with_service1.GetRequiredService<ITestService2>();
        var service2_2 = services_without_service1.GetRequiredService<ITestService2>();

        Assert.That.Value(service2_1)
           .As<Service2WithOptionalPrivateReadonlyField>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNotNull());

        Assert.That.Value(service2_2)
           .As<Service2WithOptionalPrivateReadonlyField>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNull());
    }

    /// <summary>Тестовая реализация сервиса 2 с опциональным свойством</summary>
    private class Service2WithOptionalPrivateReadonlyProperty : ITestService2
    {
        [Inject(false)]
        private ITestService1? Service1 { get; init; }

        public ITestService1? GetService1() => Service1;
    }

    /// <summary>Тестирование возможности внедрения сервиса в опциональное свойство при отсутствии сервиса в контейнере</summary>
    [TestMethod]
    public void InjectWithOptionalProperty()
    {
        var service_collection_with_service1 = new ServiceCollection();
        var service_collection_without_service1 = new ServiceCollection();

        service_collection_with_service1.AddSingleton<ITestService1>(new SimpleTestService1());

        var service1_interface = typeof(ITestService2);
        var service2_implementation = typeof(Service2WithOptionalPrivateReadonlyProperty);
        service_collection_with_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);
        service_collection_without_service1.AddService(service1_interface, service2_implementation, ServiceLifetime.Transient);

        var services_with_service1 = service_collection_with_service1.BuildServiceProvider();
        var services_without_service1 = service_collection_without_service1.BuildServiceProvider();

        var service1_1 = services_with_service1.GetService<ITestService1>();
        var service1_2 = services_without_service1.GetService<ITestService1>();

        Assert.That.Value(service1_1).IsNotNull();
        Assert.That.Value(service1_2).IsNull();

        var service2_1 = services_with_service1.GetRequiredService<ITestService2>();
        var service2_2 = services_without_service1.GetRequiredService<ITestService2>();

        Assert.That.Value(service2_1)
           .As<Service2WithOptionalPrivateReadonlyProperty>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNotNull());

        Assert.That.Value(service2_2)
           .As<Service2WithOptionalPrivateReadonlyProperty>()
           .Where(s => s.GetService1()).Check(s1 => s1.IsNull());
    }

    private class Service2WithPublicMethodInjection : ITestService2
    {
        private ITestService1? _Service1;

        [Inject]
        public void Initialize(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1? GetService1() => _Service1;
    }

    [TestMethod]
    public void InjectWithPublicMethod()
    {
        var service_interface = typeof(ITestService2);
        var service_type = typeof(Service2WithPublicMethodInjection);
        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;

        var service_collection = new ServiceCollection();
        service_collection.AddSingleton<ITestService1, SimpleTestService1>();
        service_collection.AddService(service_interface, service_type, service_lifetime);

        var descriptors = service_collection.ToArray();
        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_interface))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var service = provider.GetRequiredService<ITestService2>();

        Assert.That.Value(service).As<Service2WithPublicMethodInjection>()
           .Where(s => s.GetService1()).Check(s => s.Is<SimpleTestService1>());
    }

    private class Service2WithPublicMethodInjectionOptional : ITestService2
    {
        private ITestService1? _Service1;

        [Inject(Required = false)]
        public void Initialize(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1? GetService1() => _Service1;
    }

    [TestMethod]
    public void InjectWithPublicMethodOptional()
    {
        var service_interface = typeof(ITestService2);
        var service_type = typeof(Service2WithPublicMethodInjectionOptional);
        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;

        var service_collection = new ServiceCollection();
        service_collection.AddService(service_interface, service_type, service_lifetime);

        var descriptors = service_collection.ToArray();
        Assert.That.Value(descriptors.Length).IsEqual(1);
        Assert.That.Value(descriptors[0])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_interface))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var service = provider.GetRequiredService<ITestService2>();

        Assert.That.Value(service).As<Service2WithPublicMethodInjectionOptional>()
           .Where(s => s.GetService1()).Check(s => s.IsNull());
    }

    private class Service2WithPrivateMethodInjection : ITestService2
    {
        private ITestService1? _Service1;

        [Inject]
        private void Initialize(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1? GetService1() => _Service1;
    }

    [TestMethod]
    public void InjectWithPrivateMethod()
    {
        var service_interface = typeof(ITestService2);
        var service_type = typeof(Service2WithPrivateMethodInjection);
        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;

        var service_collection = new ServiceCollection();
        service_collection.AddSingleton<ITestService1, SimpleTestService1>();
        service_collection.AddService(service_interface, service_type, service_lifetime);

        var descriptors = service_collection.ToArray();
        Assert.That.Value(descriptors.Length).IsEqual(2);
        Assert.That.Value(descriptors[1])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_interface))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var service = provider.GetRequiredService<ITestService2>();

        Assert.That.Value(service).As<Service2WithPrivateMethodInjection>()
           .Where(s => s.GetService1()).Check(s => s.Is<SimpleTestService1>());
    }

    private class Service2WithPrivateMethodInjectionOptional : ITestService2
    {
        private ITestService1? _Service1;

        [Inject(Required = false)]
        private void Initialize(ITestService1 Service1) => _Service1 = Service1;

        public ITestService1? GetService1() => _Service1;
    }

    [TestMethod]
    public void InjectWithPrivateMethodOptional()
    {
        var service_interface = typeof(ITestService2);
        var service_type = typeof(Service2WithPrivateMethodInjectionOptional);
        const ServiceLifetime service_lifetime = ServiceLifetime.Transient;

        var service_collection = new ServiceCollection();
        service_collection.AddService(service_interface, service_type, service_lifetime);

        var descriptors = service_collection.ToArray();
        Assert.That.Value(descriptors.Length).IsEqual(1);
        Assert.That.Value(descriptors[0])
           .Where(d => d.ImplementationFactory).Check(f => f.IsNotNull("Не было сформировано фабричного метода экземпляров сервиса"))
           .Where(d => d.ImplementationInstance).Check(i => i.IsNull("Неверно был сформирован экземпляр сервиса"))
           .Where(d => d.Lifetime).Check(l => l.IsEqual(service_lifetime))
           .Where(d => d.ServiceType).Check(t => t.IsEqual(service_interface))
           .Where(d => d.ImplementationType).Check(t => t.IsNull());

        var provider = service_collection.BuildServiceProvider();

        var service = provider.GetRequiredService<ITestService2>();

        Assert.That.Value(service).As<Service2WithPrivateMethodInjectionOptional>()
           .Where(s => s.GetService1()).Check(s => s.IsNull());
    }
}