internal class Program
{
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var fields = FillGame();
        const int iComputer = 0;
        const int iUser = 1;

        var computerHits = new List<int>();
        var userHits = new List<int>();

        var userChoice = 0;
        while (fields.Length > 1)
        {
            DrowFields(fields, computerHits, userHits);
            Console.WriteLine("Where did the computer place its ship?");

            int.TryParse(Console.ReadLine(), out userChoice);
            userHits.Add(userChoice);
            if (userChoice == fields[iUser])
            {
                DrowFields(fields, computerHits, userHits);
                Console.WriteLine("You win!");

                break;
            }

            var computerChoice = new Random().Next(1, 9);
            computerHits.Add(computerChoice);
            if (computerChoice == fields[iComputer])
            {
                DrowFields(fields, computerHits, userHits);
                Console.WriteLine("Computer win!");

                break;
            }
        }

        Console.WriteLine("Thanks!");
        Console.ReadLine();


    }

    private static void DrowFields(int[] fields, List<int> computerHits, List<int> userHits)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  You Fields   ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(" Computer's Fields  ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┏━━━┳━━━┳━━━┓");

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┏━━━┳━━━┳━━━┓");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┃{0}┃{1}┃{2}┃", computerHits.Contains(1) ? DrawHit(fields[0], 1) : " 1 ", computerHits.Contains(2) ? DrawHit(fields[0], 2) : " 2 ", computerHits.Contains(3) ? DrawHit(fields[0], 3) : " 3 ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┃{0}┃{1}┃{2}┃", userHits.Contains(1) ? DrawHit(fields[1], 1) : " 1 ", userHits.Contains(2) ? DrawHit(fields[1], 2) : " 2 ", userHits.Contains(3) ? DrawHit(fields[1], 3) : " 3 ");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┣━━━╋━━━╋━━━┫");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┣━━━╋━━━╋━━━┫");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┃{0}┃{1}┃{2}┃", computerHits.Contains(4) ? DrawHit(fields[0], 4) : " 4 ", computerHits.Contains(5) ? DrawHit(fields[0], 5) : " 5 ", computerHits.Contains(6) ? DrawHit(fields[0], 6) : " 6 ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┃{0}┃{1}┃{2}┃", userHits.Contains(4) ? DrawHit(fields[1], 4) : " 4 ", userHits.Contains(5) ? DrawHit(fields[1], 5) : " 5 ", userHits.Contains(6) ? DrawHit(fields[1], 6) : " 6 ");


        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┣━━━╋━━━╋━━━┫");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┣━━━╋━━━╋━━━┫");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┃{0}┃{1}┃{2}┃", computerHits.Contains(7) ? DrawHit(fields[0], 7) : " 7 ", computerHits.Contains(8) ? DrawHit(fields[0], 8) : " 8 ", computerHits.Contains(9) ? DrawHit(fields[0], 9) : " 9 ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┃{0}┃{1}┃{2}┃", userHits.Contains(7) ? DrawHit(fields[1], 7) : " 7 ", userHits.Contains(8) ? DrawHit(fields[1], 8) : " 8 ", userHits.Contains(9) ? DrawHit(fields[1], 9) : " 9 ");

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┗━━━┻━━━┻━━━┛");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┗━━━┻━━━┻━━━┛");

        Console.ResetColor();

    }

    private static string DrawHit(int x, int hit)
    {
        if (hit == x)
        {
            return " X ";
        }
        return " • ";
    }

    private static int[] FillGame()
    {
        int userField;
        int compField;
        while (true)
        {
            Console.WriteLine("\r\n\r\nPlease select a field for a single-deck ship.");
            Console.WriteLine(@"
┏━━━┳━━━┳━━━┓
┃ 1 ┃ 2 ┃ 3 ┃
┣━━━╋━━━╋━━━┫
┃ 4 ┃ 5 ┃ 6 ┃
┣━━━╋━━━╋━━━┫
┃ 7 ┃ 8 ┃ 9 ┃
┗━━━┻━━━┻━━━┛");
            // логика создания поля
            userField = UserFildInput();

            Console.WriteLine("Field saved.");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Do you want to start game? (yes/no)");
            var answer = Console.ReadLine();
            if (answer != null && answer.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                // логика расположения корабля компьютера
                compField = RandomFieldComp();
                Console.WriteLine("Game started!");
                break;
            }
            else
            {
                return new int[0];
            }
        }
        return new int[2] { userField, compField };
    }

    private static int RandomFieldComp()
    {
        return new Random().Next(1, 10);
    }

    private static int UserFildInput()
    {

        var fieldUser = Console.ReadLine();
        int.TryParse(fieldUser, out var f);
        return f;
    }
}