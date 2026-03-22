using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using static Program;

internal class Program
{
    public class Car
    {
        public int id;
        public Car()
        {
            id = 1;
        }

    }

    public struct Truck
    {
        public Nullable<int> Id { get; set; }
        public object Weight { get; set; }
    }

    static class Bike
    {
        public static int Rama { get; set; }
    }

    class Moped
    {
        public int id;
        public int lenth;

        public Moped()
        {
            lenth = 5;
        }
    }

    record Ship (int id, string name);

    record struct PlaneRecord(int id, string name);
    record struct Plane 
    {
        public int id;
        public string name;
    }

    public class CustomArray : IEnumerable<int>
    {
        private List<int> _items = new List<int>();

        public CustomArray(params int[] el)
        {
            _items.AddRange(el);
        }

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            foreach (var item in _items)
                yield return item;
        }

        public IEnumerator GetEnumerator()
        {
            foreach (var item in _items)
                yield return item;
        }

        public void Add(int elemnt)
        {
            _items.Add(elemnt);
        }

        public int this[int index]
        {
            get
            {
                if (index < 0 || index >= _items.Count)
                    throw new IndexOutOfRangeException($"Индекс {index} вне диапазона");
                return _items[index];
            }
            set
            {
                if (index < 0 || index >= _items.Count)
                    throw new IndexOutOfRangeException($"Индекс {index} вне диапазона");
                _items[index] = value;
            }
        }

        public static CustomArray operator +(CustomArray arr1, CustomArray arr2)
        {
            int l = arr1.Count() > arr2.Count() ? arr1.Count() : arr2.Count(); 
            var arr_res = new CustomArray();

            for (int i = 0; i < l; i++)
            {
                int el1 = 0;
                int el2 = 0;
                if (arr1.Count() >= i + 1)
                {
                    el1 = arr1[i];
                }
                if (arr2.Count() >= i + 1)
                {
                    el2 = arr2[i];
                }


                arr_res.Add(el1 + el2);
            }

            return arr_res;
        }

    }
    private static string HashPassword(string password)
    {
        using var sha = SHA3_512.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static void Main(string[] args)
    {
        var apple = "apple";
        var hashApple = HashPassword(apple);


        var string111 = "";
        foreach (var s1 in args)
        {
            string111 += s1 + "\\";
        }

        var hashCode = 0;

        hashCode = string111.GetHashCode();
        int id2 = 8;
        hashCode = id2.GetHashCode();


        var dict1 = new Dictionary<Truck, int>();

        var m1 = new Truck() { Id = 9,  Weight = 8 };
        var m2 = new Truck() { Id = 9,  Weight = 8 };
        var m3 = new Truck() { Id = 29, Weight = 8 };
        var m4 = new Truck() { Id = 39, Weight = 9 };
        var m5 = new Truck() { Id = 49, Weight = 5 };


        hashCode = m1.GetHashCode();
        hashCode = m2.GetHashCode();
        hashCode = m3.GetHashCode();
        hashCode = m4.GetHashCode();
        hashCode = m5.GetHashCode();
        //HashSet hs = new HashSet<Truck>();
        dict1.Add(m1, 2);
        dict1.Add(m2, 3);
        dict1.Add(m3, 4);
        dict1.Add(m4, 5);
        dict1.Add(m5, 6);

        return;


        string111 = "";

        args.ToList().ForEach(x => string111 += x + "\\");




        var isEqual = false;

        var moped1 = new Moped();
        var moped2 = moped1;
        var moped3 = moped2;
        var moped4 = moped1;
        var moped5 = moped3;
        var moped6 = moped2;

        isEqual = object.Equals(moped1, moped2); // T
        isEqual = object.Equals(moped2, moped3); // T
        isEqual = object.Equals(moped3, moped4); // T
        isEqual = object.Equals(moped4, moped5); // T
        isEqual = object.Equals(moped5, moped6); // T

        moped2 = new Moped();

        isEqual = object.Equals(moped1, moped2); // M-F, V-F
        isEqual = object.Equals(moped2, moped3); // M-F, V-T
        isEqual = object.Equals(moped3, moped4); // M-F, V-T
        isEqual = object.Equals(moped4, moped5); // M-F, V-T
        isEqual = object.Equals(moped5, moped6); // M-F, V-T


        var truck1 = new Truck();
        var truck2 = new Truck();

        var (ship, plane) = GenerateShipAndPlane();
        plane.name = "ABC_1";
        //ship.name = "ABC_2";
        var planeR = new PlaneRecord(55, "1221");
        planeR.name = "sdf";

        TestMethod(truck1, moped1, ship, plane);

        isEqual = object.Equals(truck1, truck2);

        truck1.Weight = moped1;
        truck2.Weight = moped2;

        isEqual = object.Equals(truck1, truck2);


        //var ship1 = new Ship();
        //var ship2 = new Ship();

        //isEqual = object.Equals(ship1, ship2);

        //ship2.id = 2;
        //isEqual = object.Equals(ship1, ship2);



        
        isEqual = object.Equals(moped1, moped2);

        moped1.id = 2;
        moped2.id = 3;
        isEqual = object.Equals(moped1, moped2);



        Bike.Rama = 9;

        ChangeBikeRama(100);

        var car05 = new Car();
        var car = car05;
        car.id = 0;

        ChangeCarId(car, 40);
        ChangeCarId(car05, 40);

        isEqual = object.Equals(car, car05);
        //SimpleArray();

        //CustomArray arr1 = new( 0, 0, 0, 0 );
        //CustomArray arr2 = new( 1, 2, 3, 4, 5, 6, 7);
        //var arr_res1 = arr1 + arr2;

        //var a = "45";
        //bool isTrue = int.TryParse(a, out int element1);

        //foreach (var el in arr_res1)
        //{
        //    Console.WriteLine(el);
        //}
    }

    private static (Ship ship, Plane plane) GenerateShipAndPlane()
    {
        return (
            new Ship(33, "ACCC") { id = -33, name = "AAAA" }, 
            new Plane() { id = 33, name = "1" }
            );
    }

    private static void TestMethod(Truck truck_1, Moped moped1, Ship ship, Plane plane)
    {
        truck_1.Id = -1;
        moped1.id = -1;

       // ship.id = -1;
        plane.id = -1;
    }

    private static void ChangeCarId(Car car, int v)
    {
        car.id = v;
    }

    static void ChangeBikeRama(int value)
    {
        Bike.Rama = value;
    }


    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }



    public class Car2 {
        public string Number { get; set; } = "AE73-54KE";
       }

    private static void SimpleArray()
    {

        var points = new List<Point>
        {
            new Point { X = 2, Y = 5 },
            new Point { X = -1, Y = 3 },
            new Point { X = 4, Y = -2 },
            new Point { X = 0, Y = 0 },
            new Point { X = 3, Y = 1 }
        };

        var a = points.All(p => p.X > 1);









        var arrSting = new string[5];
        arrSting[0] = "321";
        arrSting[2] = "not empty";
        arrSting[4] = "last";

        var listString = new List<string>();
        listString.AddRange(["1", "2", "3"]);

        var firsEl = listString[0];
        listString.Add("4");

        var listLong = new List<long>();
        listLong.Add(378);
        listLong.Add(379);
        listLong.Add(380);

        var listCars = new List<Car2>();
        listCars.Add(new Car2 { Number = "AT45-43AO" });
        listCars.Add(new Car2());

        //var listOfListOfList = new List<List<List<string>>>();

        var dictionary = new Dictionary<int, string>();
        int i = 0;
        foreach (var item in listString)
        { 
            dictionary[i++] = item;
        }

        dictionary.TryGetValue(1, out var value2);
        value2 = "3000";

        var newCollection = dictionary.Values.Where(x => x == "3" || x == "4")
            .OrderByDescending(x => x)
            .Select(x => $" --- {x}")
            .ToList();
    }

    static byte[] SumArrays(byte[] arr, byte[] arr2)
    {
        //var arr = new byte[] { 0, 1, 166, 255 };
        //var arr2 = new byte[] { 1, 2, 3, 4, 5 };

        int l = arr.Length > arr2.Length ? arr.Length : arr2.Length; // тернарный оператор
        var arr_res = new byte[l];

        for (int i = 0; i < l; i++)
        {
            byte el1 = 0;
            byte el2 = 0;
            if (arr.Length >= i + 1)
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
        return arr_res;
    }

}