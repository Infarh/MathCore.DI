namespace ConsoleTests.Services;

public class NullChecker : IChecker
{
    public bool Check(object? value) => value != null;
}