

using System.Reflection;
using System.Runtime.CompilerServices;

var t = typeof(TestImpl);

var calculator = t.GetProperty("Calculator");

var get_calculator = calculator.GetMethod;
//var test = get_calculator.GetCustomAttribute<IsExternalInit>()

var checker = t.GetProperty("Checker");
var printer = t.GetProperty("Printer");

var services = new ServiceCollection();

services.AddScoped<ICalculator, Calculator>();
services.AddScoped<IChecker, NullChecker>();
services.AddScoped<IPrinter, ConsolePrinter>();
services.AddScoped<ITester, Tester>();

services.AddComposite<IManager>(ServiceLifetime.Scoped);

var provider = services.BuildServiceProvider();

var manager = provider.GetRequiredService<IManager>();

Console.WriteLine();

