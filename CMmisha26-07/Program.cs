using System;
using System.Collections.Generic;

struct Point
{
    public int X { get; set; }
    public int Y { get; set; }
}

class Car
{
    public string Name { get; set; }
    public int Year { get; set; }
    public Point Position { get; set; }

    public Car(string name, int year)
    {
        Name = name;
        Year = year;
        Position = new Point { X = 0, Y = 0 };
    }

    public void Move(int x, int y)
    {
        Position = new Point { X = x, Y = y };
    }

    public void PrintInfo()
    {
        Console.WriteLine($"{Name} ({Year}), Position: ({Position.X}, {Position.Y})");
    }
}

class Garage
{
    private List<Car> cars = new List<Car>();

    public void AddCar(Car car)
    {
        cars.Add(car);
    }

    public void PrintCars()
    {
        foreach (var car in cars)
        {
            car.PrintInfo();
        }
    }
}

class Program
{
    static void Main()
    {
        var car1 = new Car("BMW", 2020);
        var car2 = new Car("Audi", 2022);

        var garage = new Garage();
        garage.AddCar(car1);
        garage.AddCar(car2);

        garage.PrintCars();

        car1.Move(10, 20);

        Console.WriteLine(car1.Position.X);
        Console.WriteLine(car1.Position.Y);

        var position = car1.Position;
        position.X = 100;

        Console.WriteLine(car1.Position.X); 

        // Доп задание
        var car3 = car1;
        car3.Name = "Tesla";

        car1.PrintInfo();
    }
}
