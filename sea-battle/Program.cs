internal class Program
{
    private static void Main(string[] args)
    {
        var fields = FillGame();

        // кто первый ходит
        var firstStep = 0;
        //    new Random().Next(0, 1);
        //if(firstStep == 0)
        //{
        //    Console.WriteLine("You choise first!");
        //}
        //else
        //{
        //    Console.WriteLine("Computer first");
        //}

        var userChoice = 0;
        while (true)
        {

            int.TryParse(Console.ReadLine(), out userChoice);
            if (userChoice == fields[1]) {
                Console.WriteLine("You win!");
                break;
            }

            if (new Random().Next(1, 9) == fields[0])
            {
                Console.WriteLine("Computer win!");
                break;
            }
        }



        //var t = 0+(7*5);
        //Console.WriteLine("t = {0}", t); //35

        //t++;

        //Console.WriteLine("t = {0}", t); //36

        //Console.WriteLine("t = {0}", t++); //36

        //Console.WriteLine("t = {0}", ++t); //38 


        //int[][] battleField = new int[2][];
        //for (int i = 0; i< 2; i++)
        //{
        //    battleField[i] = new[] { ++t, ++t }; 
        //}

        //for (int i = 0; i < array.Length; i++) { 
        //Console.WriteLine(i);

        //}
        //Console.WriteLine(array.Length);

        //for (int i = 0; i < battleField.Length; i++)
        //{
        //    for (int j = 0; j < battleField[i].Length; j++)
        //    {
        //        Console.Write(battleField[i][j]);
        //    }
        //    Console.WriteLine();
        //}


    }

    private static int[] FillGame()
    {
        int userField;
        int compField;
        while (true)
        {
            Console.WriteLine("\r\n\r\nPlease create the field");
            // логика создания поля
            userField = UserFildInput();
            Thread.Sleep(3_000);

            Console.WriteLine("Field saved.");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Do you want to start game? (yes/no)");
            var answer = Console.ReadLine();
            if (answer != null && answer.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                // логика нашего расположения
                compField = RandomFieldComp();
                Console.WriteLine("Game started!");
                break;
            }
        }
        return new int[2] { userField, compField};
    }

    private static int RandomFieldComp()
    {
        return new Random().Next(1, 9);
    }

    private static int UserFildInput()
    {

        var fieldUser = Console.ReadLine();
        int.TryParse(fieldUser, out var f);
        Console.WriteLine("Field saved.");

        return f;
    }
}