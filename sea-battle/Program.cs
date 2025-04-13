internal class Program
{
    private const int FieldSize = 4;
    private const int P1ShipAmount = 3;
    private const int P2ShipAmount = 0;
    private const int P3ShipAmount = 0;
    private const int P4ShipAmount = 0;
    private const int P5ShipAmount = 0;
    private const int AllShipsAmout = P1ShipAmount + P2ShipAmount * 2 + P3ShipAmount * 3 + P4ShipAmount * 4 + P5ShipAmount * 5;
    private static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        var fields = FillGame();
        const int iComputer = 0;
        const int iUser = 1;

        var computerHits = new List<int>();
        var userHits = new List<int>();

        var userChoice = 0;
        while (fields.UserFields.Length > 0)
        {
            DrowFields(fields, computerHits, userHits);
            Console.WriteLine("Where did the computer place its ship?");

            int.TryParse(Console.ReadLine(), out userChoice);
            userHits.Add(userChoice);
            if (fields.ComputerFields.All(x=>userHits.Contains(x)))
            {
                DrowFields(fields, computerHits, userHits);
                Console.WriteLine("You win!");

                break;
            }

            var computerChoice = new Random().Next(1, 17);
            computerHits.Add(computerChoice);
            if (fields.UserFields.All(x => computerHits.Contains(x)))
            {
                DrowFields(fields, computerHits, userHits);
                Console.WriteLine("Computer win!");

                break;
            }
        }

        Console.WriteLine("Thanks!");
        Console.ReadLine();


    }

    private static void DrowFields((int[] UserFields, int[] ComputerFields) fields, List<int> computerHits, List<int> userHits)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("  You Fields   ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(" Computer's Fields  ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("┏━━━┳━━━┳━━━┳━━━┓");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\t");
        Console.WriteLine("┏━━━┳━━━┳━━━┳━━━┓");

        var colors = new[] { ConsoleColor.Green, ConsoleColor.DarkYellow };
        for (int i = 0; i < FieldSize; i++)
        {
            for (int u = 0; u < 2; u++)
            {
                Console.ForegroundColor = colors[u];
                var hits = u == 0 ? computerHits : userHits;
                var drawFields = u == 0 ? fields.UserFields : fields.ComputerFields;
                for (int j = 0; j < FieldSize; j++)
                {
                    int item = (i) * FieldSize + (j + 1);
                    Console.Write("┃{0}", hits.Contains(item) ? DrawHit(drawFields, item) : item.ToString().PadLeft(2).PadRight(3));
                }
                Console.Write("┃\t");
            }
            Console.WriteLine();
            for (int u = 0; u < 2; u++)
            {
                Console.ForegroundColor = colors[u];
                if (i == FieldSize - 1)
                {
                    Console.Write("┗━━━┻━━━┻━━━┻━━━┛\t");
                }
                else
                {
                    Console.Write("┣━━━╋━━━╋━━━╋━━━┫\t");
                }
            }
            Console.WriteLine();
        }

        Console.ResetColor();

    }

    private static string DrawHit(int[] fields, int hit)
    {
        if (fields.Contains(hit))
        {
            return " X ";
        }
        return " • ";
    }

    private static (int[] UserFields, int[] ComputerFields) FillGame()
    {
        int[] userFields = new int[AllShipsAmout];
        int[] compFields = new int[AllShipsAmout];

        int shipIndex = 0;

        Console.WriteLine("\r\n\r\nPlease choose a field for a single-deck ship.");
        Console.WriteLine(@"
┏━━━┳━━━┳━━━┳━━━┓
┃ 1 ┃ 2 ┃ 3 ┃ 4 ┃
┣━━━╋━━━╋━━━╋━━━┫
┃ 5 ┃ 6 ┃ 7 ┃ 8 ┃
┣━━━╋━━━╋━━━╋━━━┫
┃ 9 ┃10 ┃11 ┃12 ┃
┣━━━╋━━━╋━━━╋━━━┫
┃13 ┃14 ┃15 ┃16 ┃
┗━━━┻━━━┻━━━┻━━━┛");

        while (true)
        {
            // логика создания поля
            var userField = UserFildInput();
            var checkPosition = CheckField(userField, userFields);
            if(checkPosition == false)
            {
                Console.WriteLine("Please insert correct value");
                continue;
            }

            userFields[shipIndex++] = userField;
            if(shipIndex < AllShipsAmout) 
            { 
                Console.WriteLine("Field saved. Please choose another one");
                continue;
            }


            Console.WriteLine("Field saved.");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine("Do you want to start game? (yes/no)");
            var answer = Console.ReadLine();
            if (answer != null && answer.StartsWith("y", StringComparison.CurrentCultureIgnoreCase))
            {
                // логика расположения корабля компьютера
                compFields = RandomFieldsComp();
                Console.WriteLine("Game started!");
                break;
            }
            else
            {
                return (new int[0], new int[0]);
            }
        }

        return (userFields, compFields);
    }

    private static bool CheckField(int selectField, int[] anotherFields)
    {
        if (anotherFields.Contains(selectField))
        {
            return false;
        }

        if (selectField == 0 || selectField > FieldSize * FieldSize)
        {
            return false;
        }

        int rowIndex = selectField / FieldSize;
        int columnIndex = (selectField - 1) % FieldSize;
        if(columnIndex == 3)
        {
            rowIndex--;
        }


        // *   *   *
        // *  🚢   *
        // *   *   *

        var checkResult = true;
        // проверка отсутствия кораблей на Севере (С)
        if (checkResult && rowIndex > 0) 
        {
            if (anotherFields.Contains(selectField - FieldSize))
            {
                checkResult = false;
            }

            // СЗ
            if(checkResult && columnIndex > 0)
            {
                if (anotherFields.Contains(selectField - FieldSize - 1))
                {
                    checkResult = false;
                }
            }

            // СВ
            if (checkResult && columnIndex + 1 < FieldSize)
            {
                if (anotherFields.Contains(selectField - FieldSize + 1))
                {
                    checkResult = false;
                }
            }
        }

        // проверка отсутствия кораблей на Юге (Ю)
        if (checkResult && rowIndex + 1 < FieldSize)
        {
            if(anotherFields.Contains(selectField + FieldSize))
            {
                checkResult = false;
            }

            // ЮЗ
            if (checkResult && columnIndex > 0)
            {
                if (anotherFields.Contains(selectField + FieldSize - 1))
                {
                    checkResult = false;
                }
            }

            // ЮВ
            if (checkResult && columnIndex + 1 < FieldSize)
            {
                if (anotherFields.Contains(selectField + FieldSize + 1))
                {
                    checkResult = false;
                }
            }
        }

        // проверка отсутствия кораблей на Западе (З)
        if (checkResult && columnIndex > 0)
        {
            if (anotherFields.Contains(selectField - 1))
            {
                checkResult = false;
            }
        }

        // проверка отсутствия кораблей на Востоке (В)
        if (checkResult && columnIndex + 1 < FieldSize)
        {
            if (anotherFields.Contains(selectField + 1))
            {
                checkResult = false;
            }
        }


        return checkResult;
    }

    private static int[] RandomFieldsComp()
    {
        var fields = new int[AllShipsAmout];
        var maxSize = FieldSize * FieldSize;
        var shipIndex = 0;
        while (shipIndex < AllShipsAmout)
        {
            var t = new Random().Next(1, maxSize + 1);
            if (CheckField(t, fields))
            {
                fields[shipIndex++] = t;
            }
            continue;
        }

        return fields;
    }

    private static int UserFildInput()
    {
        var fieldUser = Console.ReadLine();
        int.TryParse(fieldUser, out var f);
        return f;
    }
}