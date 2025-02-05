
using ConsoleTests.ViewModels;

var services = new ServiceCollection();

services.AddTransient<IService0, Service0>();
var provider = services.BuildServiceProvider();
var collection = provider.GetService<IEnumerable<ServiceDescriptor>>();


//services.AddServicesFromAssembly(typeof(Program));



var model = provider.GetRequiredService<MainViewModel>();

Console.WriteLine();

interface IService0
{

}

internal class Service0 : IService0
{
}
