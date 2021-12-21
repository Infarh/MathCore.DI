namespace MathCore.DI.Tests.Interfaces;

public interface ITestCompositor
{
    ITestService1 Service1 { get; }
    ITestService2 Service2 { get; }
    ITestService3 Service3 { get; set; }
}