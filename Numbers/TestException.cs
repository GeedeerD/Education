using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numbers
{
    internal class TestException : Exception
    {
        public TestException(string messge) : base(messge) { }
    }
}
