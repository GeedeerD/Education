using System;

class Animal
{
    // Инкапсуляция
    private string name;

    public Animal(string name)
    {
        this.name = name;
    }

    public string GetName()
    {
        return name;
    }

    // Метод для полиморфизма
    public virtual void MakeSound()
    {
        Console.WriteLine("Животное издает звук");
    }
}

// Наследование
class Dog : Animal
{
    public Dog(string name) : base(name) { }

    // Полиморфизм
    public override void MakeSound()
    {
        Console.WriteLine("Гав-гав!");
    }
}

class Cat : Animal
{
    public Cat(string name) : base(name) { }

    // Полиморфизм
    public override void MakeSound()
    {
        Console.WriteLine("Мяу-мяу!");
    }
}

class Program
{
    static void Main()
    {
        Dog dog = new Dog("Бобик");
        Cat cat = new Cat("Мурка");

        Console.WriteLine($"Собака: {dog.GetName()}");
        Console.WriteLine($"Кот: {cat.GetName()}");

        Animal[] animals = { dog, cat };

        foreach (Animal animal in animals)
        {
            animal.MakeSound();
        }
    }
}
