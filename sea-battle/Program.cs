using System;

internal class Program
{
    private static void Main(string[] args)
    {
        var fields = FillGame();
        int userField = fields[0]; // Корабль игрока
        int compField = fields[1]; // Корабль компьютера
        Random random = new Random();

        Console.WriteLine("\nGame started! Try to guess the computer's ship position.");

        while (true)
        {
            // Ход игрока
            Console.Write("\nYour turn: ");
            if (!int.TryParse(Console.ReadLine(), out var userChoice) || userChoice < 1 || userChoice > 16)
            {
                Console.WriteLine("Invalid input. Enter a number between 1 and 16.");
                continue;
            }

            if (userChoice == compField)
            {
                Console.WriteLine("You win! ");
                break;
            }
            else
            {
                Console.WriteLine("Miss! Computer's turn...");
            }

            // Ход компьютера
            int botChoice = random.Next(0, 17);
            Console.WriteLine($"Computer shoots at {botChoice}");

            if (botChoice == userField)
            {
                Console.WriteLine("Computer win! ");
                break;
            }
            else
            {
                Console.WriteLine("Computer missed!");
            }
        }
    }

    private static int[] FillGame()
    {
        int userField;
        int compField;

        while (true)
        {
            Console.Write("\n\nWhere do you want to set your ship (1-16)? ");
            userField = UserFieldInput();
            Console.WriteLine(@"
┏━━━┳━━━┳━━━┳━━━┓
┃ 1 ┃ 2 ┃ 3 ┃ 4 ┃
┣━━━╋━━━╋━━━┫━━━┫
┃ 5 ┃ 6 ┃ 7 ┃ 8 ┃
┣━━━╋━━━╋━━━┫━━━┫
┃ 9 ┃ 10┃ 11┃ 12┃
┣━━━╋━━━╋━━━╋━━━┫
┃ 13┃ 14┃ 15┃ 16┃
┗━━━┻━━━┻━━━┻━━━┛");

            Console.Write("Do you want to start the game? (yes/no): ");
            var answer = Console.ReadLine();
            if (answer?.StartsWith("y", StringComparison.CurrentCultureIgnoreCase) == true)
            {
                compField = RandomFieldComp();
                break;
            }
        }

        return new int[2] { userField, compField };
    }

    private static int RandomFieldComp()
    {
        return new Random().Next(1, 16);
    }

    private static int UserFieldInput()
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out var f) && f >= 1 && f <= 16)
            {
                Console.WriteLine("Field saved.");
                return f;
            }

            Console.Write("Invalid input. Enter a number between 1 and 16: ");
        }
    }
}