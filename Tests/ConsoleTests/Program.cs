
using MathCore.DI;

var services = new ServiceCollection();

services.AddScoped<ICalculator, Calculator>();
services.AddScoped<IChecker, NullChecker>();
services.AddScoped<IPrinter, ConsolePrinter>();
services.AddScoped<ITester, Tester>();

services.AddComposite<IManager>(ServiceLifetime.Scoped);