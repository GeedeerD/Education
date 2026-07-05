using System.Drawing;
using Models;
using Numbers;

internal class Program
{
    public class Car
    {
        public string Name { get; set; }
        public Car(string name)
        {
            Name = name;
        }
    }
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        int number_1 = -1000;
        int number_2 = 8000;
        Swap(ref number_1, ref number_2);
        Console.WriteLine(number_1);


        string str_1 = "Строка 1";
        string str_2 = "Строка 2";

        Swap(ref str_1, ref str_2);
        Console.WriteLine(str_1);

        Car car_1 = new Car("Авто 1");
        Car car_2 = new Car("Авто 2");

        Swap(ref car_1, ref car_2);
        Console.WriteLine(car_1.Name);

        SwapCarNames(car_1, car_2);
        Console.WriteLine(car_1.Name);


    }

    private static void Swap(ref int number_1, ref int number_2)
    {
        int tmp = number_1;
        number_1 = number_2;
        number_2 = tmp;
    }

    private static void Test(params int[] p)
    {

    }
    private static void Swap(ref string str_1, ref string str_2)
    {
        string tmp = str_1;
        str_1 = str_2;
        str_2 = tmp;
    }

    private static void Swap(ref Car car_1, ref Car car_2)
    {
        Car tmp = car_1;
        car_1 = car_2;
        car_2 = tmp;
    }

    private static void SwapCarNames(Car car_1, Car car_2)
    {
        string tmp = car_1.Name;
        car_1.Name = car_2.Name;
        car_2.Name = tmp;
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