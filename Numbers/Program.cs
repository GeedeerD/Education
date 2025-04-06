using System.Diagnostics;
using Enums.Types;

internal class Program
{
    private static void Main(string[] args)
    {
        var sw = Stopwatch.StartNew();
        GetMinNumbers(14, 15_000_000);

        Console.WriteLine(sw.ElapsedMilliseconds);
        Console.ReadLine();
        return;

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

    private static void ChangeParam(int x)
    {
        //throw new TestException("This method only for test");
        x = 1;
    }
}