using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Enums.Types;
using Models;
using Models.Messages;
using Numbers;

internal class Program
{
    private static void Main(string[] args)
    {
        TestMethod1();
        var testItem1 = new TestItem1();
        var testStruct1 = new TestStruct1();

        //var pen = new Pen();
        //var steelId = (int)Material.Steel;

        //S.Write("asd");

        //var aaa = new A();
        //aaa.Name = "asd";
        //var bbbb = new B();

        //var ccc = new C();


        //if (aaa is Object) {
        //}

        //var color = bbbb.Color;
        //var aa1 = bbbb as A;
        //var color1 = ((B)aa1).Color;


        //var id = ((B)ccc).Id;
            
        //return;


        IItem[] biTems = new BaseItem[5];

        biTems[0] = new TestItem0();
        biTems[1] = new TestItem1();
        biTems[2] = new TestItem1() { Id = 188 };

        //string name = 1;

        return;











        var pen = new TestPen(); // Green
        var printer = new Printer3D(); // Yellow


        var rc = new RealClass("Test");
        //var i1 = new IInterface
        var i1 = (IInterface1)rc;
        var i2 = (IInterface2)rc;

        return;
        string toChtoNadaNapisat = "{0}, {1}, {2}, {3}";

        int magicValue = 5363;
        ChangeParam(magicValue);


        TestString(ref toChtoNadaNapisat);
        //TestString(toChtoNadaNapisat2);

        int a2 = 234 + (Int16)LayerType.First;
        Console.Write(toChtoNadaNapisat, LayerType.First, LayerType.Second, LayerType.Third, a2);

        return;
        var sw = Stopwatch.StartNew();
        GetMinNumbers(14, 15_000_000);

        Console.WriteLine(sw.ElapsedMilliseconds);
        Console.ReadLine();
        return;
        var r = -3;

        // | - обязательный ИЛИ
        // & - обязательное И

        if (r > 4 || CheckLowerThanZero(r))
        {
            CancellationToken ct = CancellationToken.None;

            if (true | ct.IsCancellationRequested | false)
            {

            }

            if (((1 == 0 || false) || false) || false)
            {
                Console.WriteLine("You Win $10000000000000!");
            }

            if (r < 100)
            {
                Console
                    .WriteLine($"r < 100:{r}");
            }
            else if (r > 100)
            {
                Console
                    .WriteLine($"r > 100:{r}");
            }
            else
            {
                Console
                    .WriteLine($"r == 100:{r}");
            }
        }
        else
        {
            Console.WriteLine($"!!!r:{r}");
        }


        var r1 = 4;
        var r2 = 16;
        var r3 = 1 + 16 - 1;
        //Console.WriteLine($"r3:{r3}");

        Point3DPtinter point3dp = new(4, -6);
        var printer3d = new Printer3D();
        printer3d.StartPosition = point3dp;
        point3dp.X = 33;
        TestStructMethod(point3dp);
        TestClassMethod(printer3d);

        Console.WriteLine("Done");

        var color = ColorFlag.Red | ColorFlag.Green;
        var colors = new object[] { ColorFlag.Red, ColorFlag.Green, ColorFlag.Black, };

        Console.WriteLine(color);

        var obj = new char[900];
        bool? a = null;

        Random rnd = new Random();
        //rnd.Next(2, 0);
        var x = 0;
        ChangeParam(x);
        var b = 4 / x;

        char? nullChar = null;
        string nullStr = null;
        string emptyStr = "";
        string emptyStr1 = string.Empty;

        var str = "sdkjakjd";
        for (int i = 0; str.Length > i; i++)
        {
            var c = str[i];
            obj[i] = c;
            Console.Write(c);
            Console.WriteLine((int)c);
        }

        //obj[8] = str;
    }

    private static void TestMethod1()
    {
        List<TestStruct1> list = new List<TestStruct1>();
        foreach(var i in Enumerable.Range(1, 1000000))
        {
            list.Add(new TestStruct1() { Id = 011111 + 1});
        }

        TestStruct1 testStruct1 = new() { Id = 3 };
        TestMethod3(testStruct1);

        Console.WriteLine("Used Memory Point");
    }

    private static void TestMethod3(TestStruct1 testStruct1)
    {
        testStruct1.Id = 12123;
    }

    private static bool CheckLowerThanZero(int r)
    {
        return r < 0;
    }

    static void TestClassMethod(Printer3D a)
    {
        var newPoint = new Point3DPtinter();
        newPoint.X = 444;
        newPoint.Y = 888;
        a.StartPosition = newPoint;

    }


    static void TestStructMethod(Point3DPtinter point3d)
    {
        point3d.X = -67;
        point3d.Y = -71;
    }

    private static void GetMinNumbers(int num, int iterations)
    {

        int minI = 0;
        int minNumbers = 10000;
        for (int i = 1; i <= iterations; i++)
        {
            var a = GetSumNumbers(num * i);
            if (a <= minNumbers)
            {
                minNumbers = a;
                minI = i;
                Console.WriteLine($"Min number: {minNumbers} - {minI} \t {num * i}");
            }
        }
    }
    private static int GetSumNumbers(int a)
    {
        int sum = 0;
        foreach (char c in a.ToString())
        {
            sum += int.Parse(c.ToString());
        }
        return sum;
    }

    private static void TestDelegate(Action<string, string> runMe)
    {
        runMe("yes", "YES!!!");
    }



    private static void TestFuncDelegate(Func<string, string, int> runMe)
    {
        var result = runMe("yes", "YES!!!");
    }

    private static void ChangeParam(int x)
    {
        x = 1;
    }
    private static void TestString(ref string aa)
    {
        aa = "";
    }


    public class Pen
    {
        public Material Meaterial { get; set; }
    }
    public class Wall
    {

    }

    public class Pan
    {
        public Bottom Bottom { get; set; }
        public Pen[] Pens { get; set; }
        public Wall Wall { get; set; }

    }


    struct St
    {
        public int X;
        public int Y;

        public int Add()
        {
            return X + Y;
        }
    }



    struct St2
    {
        public int X;
        public int Y;

        public int Add()
        {
            return X + Y;
        }
    }

    static class S
    {
        public static void Write(string a)
        {

        }
    }

    class A
    {
        public A()
        {
            Console.WriteLine("A created");
        }

        public A(int i)
        {
            Id = i;
        }
        public int Id { get; set; }
        public string Name { get; set; }
    }

    class C : B
    {

    }

    class B : A
    {
        public B() : base()
        {
            Console.WriteLine("B created");
        }


        public Color Color { get; set; }
    }

}

public enum Material : int //
{
    Steel,
    Plastic,
    Glass
}