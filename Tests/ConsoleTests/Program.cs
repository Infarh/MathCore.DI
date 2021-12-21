
using ConsoleTests.ViewModels;

var services = new ServiceCollection();

services.AddServicesFromAssembly(typeof(Program));

var provider = services.BuildServiceProvider();

var model = provider.GetRequiredService<MainViewModel>();

Console.WriteLine();

