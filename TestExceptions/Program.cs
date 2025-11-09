using Exceptions;

internal class Program
{
    private static void Main(string[] args)
    {
        Console.Write("Please input a positive number:");
        string value = null;

        var timeout = 20;
        var client = new HttpClient();
        while (value == null)
        {
            try
            {

                try
                {
                    //value = Console.ReadLine();
                    // DoRetry

                    CancellationToken ct = new CancellationTokenSource(timeout).Token;
                    var result = client.GetAsync("https://www.google.com", ct).GetAwaiter().GetResult();
                    var googleForm = result.Content.ReadAsStringAsync().GetAwaiter().GetResult();


                    var intValue = Converter.ParseToInt(value);
                }
                catch (TaskCanceledException ex)
                {
                    timeout = 4000;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    throw;
                    Console.Write("Please input a positive number:");
                    value = null;
                }
                finally
                {
                    Console.WriteLine(value);
                }
            }
            catch 
            { 
                
            }
        }
    }
}


public static class Converter
{
    public static int ParseToInt(string value)
    {
        foreach(var c in value.ToArray())
        {
            if(c!='0' || c != '1'|| c != '2' || c != '3' || c != '4' || c != '5' || c != '6' || c != '7' || c != '8' || c != '9')
            {
                throw new OnlyPositiveNumbersException("Wrong number", value);
            }
        }

        return int.Parse(value);
    }
}