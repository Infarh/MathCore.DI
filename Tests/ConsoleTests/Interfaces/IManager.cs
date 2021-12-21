namespace ConsoleTests.Interfaces;

public interface IManager
{
    ICalculator Calculator { get; init; }

    IChecker Checker { get; }

    IPrinter Printer { get; set; }

    ITester Tester { get; set; }
}