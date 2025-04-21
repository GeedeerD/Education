using System;

class Program
{
    static void Main()
    {
        //DZ01();
        //DZ02();
        //DZ03();
        //DZ04();
        //DZ05();
        //DZ06();
        //DZ07();
        //DZ08();
        //DZ09();
        //DZ10();
    }

    static void DZ01()
    {
        
    }

    struct Point
    {
        public int X, Y;
        public Point(int x, int y) { X = x; Y = y; }
        public void Increment() { X++; Y++; }
        public void Decrement() { X--; Y--; }
        public void Display() => Console.WriteLine($"X = {X}, Y = {Y}");
    }

    static void DZ02()
    {
        Console.WriteLine("********* A First Look at Structures *****\n");
        Point myPoint;
        myPoint.X = 349;
        myPoint.Y = 76;
        myPoint.Display();
        myPoint.Increment();
        myPoint.Display();
    }

    static void DZ03()
    {
        Point p1 = new Point();
        p1.X = 10;
        p1.Y = 0;
        p1.Display();

        Point p2;
        p2.X = 10;
        p2.Y = 10;
        p2.Display();

        p1.Display();
    }

    static void DZ04()
    {
        ReadOnlyPoint point = new ReadOnlyPoint(5, 10);
        point.Display();
    }

    struct ReadOnlyPoint
    {
        public int X { get; }
        public int Y { get; }

        public ReadOnlyPoint(int xPos, int yPos)
        {
            X = xPos;
            Y = yPos;
        }

        public void Display()
        {
            Console.WriteLine($"X = {X}, Y = {Y}");
        }
    }

    class PointRef
    {
        public int X, Y;
        public PointRef(int x, int y) { X = x; Y = y; }
        public void Display() => Console.WriteLine($"X = {X}, Y = {Y}");
    }

    static void DZ05()
    {
        int i = 0;
        Point p = new Point();
        Console.WriteLine($"Local int i = {i}");
        p.Display();
    }

    static void DZ06()
    {
        Console.WriteLine("Assigning value types\n");
        Point p1 = new Point(10, 10);
        Point p2 = p1;
        p1.Display();
        p2.Display();
        p1.X = 100;
        Console.WriteLine("\n=> Changed p1.X\n");
        p1.Display();
        p2.Display();
    }

    static void DZ07()
    {
        Console.WriteLine("Assigning reference types\n");
        PointRef p1 = new PointRef(10, 10);
        PointRef p2 = p1;
        p1.Display();
        p2.Display();
        p1.X = 100;
        Console.WriteLine("\n=> Changed p1.X\n");
        p1.Display();
        p2.Display();
    }

    class Shapelnfo
    {
        public string InfoString;
        public Shapelnfo(string info) => InfoString = info;
    }

    struct Rectangle
    {
        public Shapelnfo RectInfo;
        public int RectTop, RectLeft, RectBottom, RectRight;

        public Rectangle(string info, int top, int left, int bottom, int right)
        {
            RectInfo = new Shapelnfo(info);
            RectTop = top;
            RectBottom = bottom;
            RectLeft = left;
            RectRight = right;
        }

        public void Display()
        {
            Console.WriteLine($"String = {RectInfo.InfoString}, Top = {RectTop}, Bottom = {RectBottom}, Left = {RectLeft}, Right = {RectRight}");
        }
    }

    static void DZ08()
    {
        Console.WriteLine("-> Creating r1");
        Rectangle r1 = new Rectangle("First Rect", 10, 10, 50, 50);
        Console.WriteLine("-> Assigning r2 to r1");
        Rectangle r2 = r1;
        Console.WriteLine("-> Changing values of r2");
        r2.RectInfo.InfoString = "This is new info!";
        r2.RectBottom = 4444;
        r1.Display();
        r2.Display();
    }

    class Person
    {
        public string personName;
        public int personAge;

        public Person(string name, int age)
        {
            personName = name;
            personAge = age;
        }

        public Person() { }

        public void Display() => Console.WriteLine($"Name: {personName}, Age: {personAge}");
    }

    static void SendAPersonByValue(Person p)
    {
        p.personAge = 99;
        p = new Person("Nikki", 99);
    }

    static void DZ09()
    {
        Console.WriteLine("Passing Person object by value");
        Person fred = new Person("Fred", 12);
        Console.WriteLine("\nBefore by value call, Person is:");
        fred.Display();
        SendAPersonByValue(fred);
        Console.WriteLine("\nAfter by value call, Person is:");
        fred.Display();
    }

    static void SendAPersonByReference(ref Person p)
    {
        p.personAge = 555;
        p = new Person("Nikki", 999);
    }

    static void DZ10()
    {
        Console.WriteLine("Passing Person object by reference");
        Person mel = new Person("Mel", 23);
        Console.WriteLine("Before by ref call, Person is:");
        mel.Display();
        SendAPersonByReference(ref mel);
        Console.WriteLine("After by ref call, Person is:");
        mel.Display();
    }
}
