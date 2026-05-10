using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Numbers
{
    internal abstract class BaseItem : IItem
    {
        public int id { get => 0; set => id = value; }
    }
    internal interface IItem { int id { get; set; } }
    internal interface IItem1 { }
    internal interface IItem2 { }
    internal interface IItem3 { }
    internal interface IItem4 { }
    internal class TestItem0 : BaseItem, IItem, IItem1, IItem2, IItem3, IItem4;
    internal class TestItem1 : BaseItem
    {
        //конструктор, финализатор, статический конструктор, вложенный тип, операция, метод, свойство, индексатор, поле, поле только для чтения, константа, событие.
        public TestItem1()
        {
            Id = 1;
        }

        public int Id { get; set; }

        ~TestItem1()
        {
            Console.WriteLine("Item disposed");
        }
    }

    public struct TestStruct1
    {
        public TestStruct1()
        {
            Id = 1;
        }
        public int Id { get; set; }
    }


}
