using ConsoleTests.Interfaces;

namespace ConsoleTests.Services;

public class Tester : ITester
{
    private readonly string _TestField = "213";

    public bool Test() => true;
}