//using System;

//class Car
//{
//    private string name;
//    private int currSpeed;

//    public Car(string n, int s)
//    {
//        name = n;
//        currSpeed = s;
//    }

//    public void Accelerate(int delta) => currSpeed += delta;

//    public void PrintState() =>
//        Console.WriteLine($"{name} drive {currSpeed} km per hour ");
//}

//class Program
//{
//    static void Main(string[] args)
//    {
//        Car myCar = new Car("BMW", 60);
//        myCar.Accelerate(20);
//        myCar.PrintState();
//    }
//}
// Инкапсуляция — это принцип ООП, при котором данные и методы, работающие с ними, объединяются в единый объект
// (класс), а доступ к внутреннему состоянию объекта ограничивается. Это позволяет скрыть детали реализации и защитить данные от некорректного использования.
// В этом примере класс Car инкапсулирует данные (name и currSpeed) и методы (Accelerate и PrintState), которые работают с этими данными. Поля name и currSpeed являются приватными, что ограничивает доступ к ним извне класса.
// Методы Accelerate и PrintState предоставляют интерфейс для взаимодействия
// с объектом Car, позволяя изменять скорость и выводить состояние автомобиля без прямого доступа к его внутренним данным.

using System;
namespace dz
{
     class Animal
    {
        public virtual void Speak()
        {
            Console.WriteLine("...");
        }
    }

    class Dog : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("Woof!");
        }
    }

    class Cat : Animal
    {
        public override void Speak()
        {
            Console.WriteLine("meow!");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Animal a1 = new Dog();
            Animal a2 = new Cat();
            var a = new[] { a1, a2 };
            a1.Speak();
            a2.Speak();
        }
    }
}
// Полимирфизм это способность объектов разных классов реагировать на один и тот же метод по-разному. В данном примере, класс Animal имеет метод Speak(),
// который переопределяется в классах Dog и Cat. Когда мы вызываем Speak() на объекте типа Animal, фактический метод, который будет вызван, зависит от типа объекта (Dog или Cat).
// Это позволяет использовать один и тот же
// интерфейс (метод Speak()) для работы с разными типами объектов, обеспечивая гибкость и расширяемость кода.

//using System;z


//class Animal
//{
//    public string Name { get; set; }

//    public void Eat()
//    {
//        Console.WriteLine($"{Name} ест");
//    }
//}


//class Dog : Animal
//{

//    public void Fetch()
//    {
//        Console.WriteLine($"{Name} приносит мяч");
//    }
//}

//class Program
//{
//    static void Main(string[] args)
//    {
//        Dog d = new Dog();
//        d.Name = "Рекс";

//        d.Eat();    
//        d.Fetch();  
//    }
//}
// Унаследование — это принцип ООП, при котором один класс (наследник) может использовать свойства и методы другого класса (родителя). В данном примере класс Dog наследует от класса Animal,
// что позволяет объекту Dog использовать метод Eat() и свойство Name, определенные в классе Animal. Это позволяет создавать более специализированные классы на основе общих,
// повторно используя код и обеспечивая иерархию объектов.