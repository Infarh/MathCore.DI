namespace ConsoleTests;

public class TestImpl(ICalculator Calculator, IChecker Checker, IPrinter Printer, ITester Tester)
    : IManager
{
    public ICalculator Calculator { get; init; } = Calculator;
    public IChecker Checker { get; } = Checker;
    public IPrinter Printer { get; set; } = Printer;
    public ITester Tester { get; set; } = Tester;
}