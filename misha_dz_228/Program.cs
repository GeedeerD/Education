using System;

class Program
{
    static void Main()
    {
        DZ01();
    }

    static void DZ01()
    {
        for (int number = 9999; number >= 1000; number--)
        {
            int sum = SumOfDigits(number);
            if (number % sum == 0)
            {
                Console.WriteLine($"Найбільше чотиризначне число, що ділиться на суму своїх цифр: {number}");
                break;
            }
        }
    }
    static int SumOfDigits(int n)
    {
        int sum = 0;
        while (n > 0)
        {
            sum += n % 10;
            n /= 10;
        }
        return sum;
    }
}
