using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestImpl : IManager
    {
        public TestImpl(ICalculator Calculator, IChecker Checker, IPrinter Printer, ITester Tester)
        {
            this.Calculator = Calculator;
            this.Checker = Checker;
            this.Printer = Printer;
            this.Tester = Tester;
        }

        public ICalculator Calculator { get; init; }
        public IChecker Checker { get; }
        public IPrinter Printer { get; set; }
        public ITester Tester { get; set; }
    }
}
