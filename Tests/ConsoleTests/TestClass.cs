using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTests
{
    public class TestClass
    {
        private string _Test1;
        public string Test1 { get => _Test1; set => _Test1 = value; }
        
        private int _Test2;
        public int Test2 { get => _Test2; set => _Test2 = value; }
        
        private bool _Test3;
        public bool Test3 { get => _Test3; set => _Test3 = value; }
        
        private List<string> _Test4;
        public List<string> Test4 { get => _Test4; set => _Test4 = value; }

        public TestClass(string Test1, int Test2, bool Test3, List<string> Test4)
        {
            _Test1 = Test1;
            _Test2 = Test2;
            _Test3 = Test3;
            _Test4 = Test4;
        }
    }
}
