using ConsoleTests.Interfaces;

namespace ConsoleTests.Services;

public class ConsolePrinter : IPrinter
{
    public void Print(string Message) => Console.WriteLine(Message);
}