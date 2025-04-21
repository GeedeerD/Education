using System;
using System.Runtime.InteropServices;

namespace Codec_
{
    class Program
    {
        public static object ProgramHelpers { get; private set; }

        static object Main() => ProgramHelpers;//TestIf();//TestSwitch();//TestLoop();//TestLoopBreak();

        static void TestLoopBreak()
        {
            int number1 = 10;

            // return - не используется

            // break
            for (int i = 0; i < 15; i++)
            {
                if (i == number1)
                {
                    break;
                }
                Console.WriteLine(i);
            }

            // continue
            for (int i = 0; i < 15; i++)
            {
                if (i == number1)
                {
                    continue;
                }
                Console.WriteLine(i);
            }
        }

        static bool TestLoop()
        {
            int number1 = 6;

            // while
            while (number1 > 0)
            {
                Console.WriteLine(number1);
                number1 = number1 - 1;
            }

            // do while
            number1 = 6;
            do
            {
                Console.WriteLine(number1--);
            } while (number1 > 0);

            // for I
            for (int i = 0; i < 6; i++)
            {
                Console.WriteLine(i);
            }

            Console.WriteLine("for II");

            // for II
            number1 = 6;
            for (; number1-- > 0;)
            {
                Console.WriteLine(number1);
            }

            return false;
        }

        static bool TestSwitch()
        {
            int number1 = -3;

            switch (number1)
            {
                case 2:
                    Console.WriteLine("2");
                    break;
                case 3:
                    Console.WriteLine("3");
                    break;
                case 4:
                    Console.WriteLine("4");
                    break;
                case 5:
                    Console.WriteLine("5");
                    break;
                default:
                    Console.WriteLine("UNKNOWN");
                    break;
            }

            return false;
        }

        static bool TestIf()
        {
            int number1 = 4;

            if (number1 == 5)
            {
                Console.WriteLine("= 5");
            }
            else if (number1 == 6)
            {
                Console.WriteLine("= 6");
            }
            else if (number1 == 7)
            {
                Console.WriteLine("= 7");
            }
            else if (number1 == 8)
            {
                Console.WriteLine("= 8");
            }
            else
            {
                Console.WriteLine("UNKNOWN");
            }

            // дополнительные условия
            if (true)
            {
                Console.WriteLine("TRUE");
            }
            else if (true || false)
            {
                Console.WriteLine("TRUE || FALSE");
            }
            else
            {
                Console.WriteLine("ELSE");
            }

            if (true)
            {
                if (false)
                {
                    if (false)
                    {
                        Console.WriteLine("false 1");
                    }
                    else if (false || true)
                    {
                        if (true)
                        {
                            // пусто
                        }
                    }
                }
            }
            else
            {
                // пусто
            }

            return true;
        }




        struct Point
        {
            public int x;
            public int y;

            public void Increment()
            {
                x++;
                y++;
            }
            public void Decrement()
            {
                x--;
                y--;
            }
            public void Print()
            {
                Console.WriteLine("x = {0}, y = {1}", x, y);
            }






        }





    }
}