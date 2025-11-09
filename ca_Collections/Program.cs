internal class Program
{
    private static void Main(string[] args)
    {
        var arr = new byte[] {  0, 1, 166, 255    };
        var arr2 = new byte[] { 1, 2,   3,   4, 5 };

        int l = arr.Length > arr2.Length ? arr.Length : arr2.Length; // тернарный оператор
        var arr_res = new byte[l];

        for (int i = 0; i < l; i++)
        {
            byte el1 = 0;
            byte el2 = 0;
            if(arr.Length >= i + 1)
            {
                el1 = arr[i];
            }
            if (arr2.Length >= i + 1)
            {
                el2 = arr2[i];
            }


            arr_res[i] = (byte)(el1 + el2);
        }

        Console.WriteLine(arr_res.ToList().ToString());
        //arr_res[1] = (byte)(arr[1] + arr2[1]);
        //arr_res[2] = (byte)(arr[2] + arr2[2]);
        //arr_res[3] = (byte)(arr[3] + arr2[3]);
        // 255      255 
        // 256 =>     0
        // 257 =>     1
        // 258 =>     2
        // 259 =>     3
    }
}