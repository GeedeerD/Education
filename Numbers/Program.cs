using Enums.Types;
using Numbers;

internal class Program
{
    private static void Main(string[] args)
    {

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

    private static void ChangeParam(int x)
    {
        //throw new TestException("This method only for test");
        x = 1;
    }
}