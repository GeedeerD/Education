using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numbers
{
    public class ArrayUserException : Exception
    {
        public ArrayUserException()
        {

        }
        public ArrayUserException(string messge) : base(messge) { }

        public int CountOfElements {get;set;}
    }
}
