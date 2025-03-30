using System.Drawing;
using Enums.Types;
using Models;
using Numbers;

internal class Program
{
    private static void Main(string[] args)
    {
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

    private static void ChangeParam(int x)
    {
        //throw new TestException("This method only for test");
        x = 1;
    }
}