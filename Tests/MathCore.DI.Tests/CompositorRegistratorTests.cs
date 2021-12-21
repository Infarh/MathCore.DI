using MathCore.DI.Tests.Interfaces;

namespace MathCore.DI.Tests;

[TestClass]
public class CompositorRegistratorTests
{
    private class TestService1 : ITestService1 { }

    private class TestService2 : ITestService2 { }

    private class TestService3 : ITestService3 { }

    private interface ITestService0
    {
        ITestService1? GetService1();
        ITestService2? GetService2();
        ITestService3? GetService3();
    }

    private class TestService0 : ITestService0
    {
        [Inject]
        private readonly ITestCompositor? _Compositor = null;

        public ITestService1? GetService1() => _Compositor?.Service1;

        public ITestService2? GetService2() => _Compositor?.Service2;

        public ITestService3? GetService3() => _Compositor?.Service3;
    }

    [TestMethod]
    public void CompilationTest()
    {
        var service_collection = new ServiceCollection();

        var service1 = new TestService1();
        var service2 = new TestService2();
        var service3 = new TestService3();

        service_collection.AddSingleton<ITestService1>(service1);
        service_collection.AddSingleton<ITestService2>(service2);
        service_collection.AddSingleton<ITestService3>(service3);

        service_collection.AddComposite<ITestCompositor>();

        service_collection.AddService(typeof(ITestService0), typeof(TestService0), ServiceLifetime.Transient);

        var descriptors = service_collection.ToArray();

        Assert.That.Value(descriptors.Length).IsEqual(5);

        var composite_descriptor = descriptors[3];
        var compositor_type = composite_descriptor.ImplementationType;
        Assert.IsNotNull(compositor_type);

        var composite_type_properties = compositor_type.GetProperties();
        Assert.That.Collection(composite_type_properties)
           .Contains(p => p.PropertyType == typeof(ITestService1))
           .Contains(p => p.PropertyType == typeof(ITestService2))
           .Contains(p => p.PropertyType == typeof(ITestService3));

        var constructor = compositor_type.GetConstructors().FirstOrDefault();
        Assert.IsNotNull(constructor);

        var constructor_parameters = constructor.GetParameters();
        Assert.That.Value(constructor_parameters)
           .Where(parameters => parameters.Length).Check(length => length.IsEqual(3))
           .Where(parameters => parameters[0]).Check(parameter => parameter.Where(p => p.ParameterType).CheckEquals(typeof(ITestService1)))
           .Where(parameters => parameters[1]).Check(parameter => parameter.Where(p => p.ParameterType).CheckEquals(typeof(ITestService2)))
           .Where(parameters => parameters[2]).Check(parameter => parameter.Where(p => p.ParameterType).CheckEquals(typeof(ITestService3)));

        var provider = service_collection.BuildServiceProvider();

        var serivce = provider.GetRequiredService<ITestService0>();

        Assert.That.Value(serivce)
           .Where(s => s.GetService1()).CheckEquals(service1)
           .Where(s => s.GetService2()).CheckEquals(service2)
           .Where(s => s.GetService3()).CheckEquals(service3);
    }
}