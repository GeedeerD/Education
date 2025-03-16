using System.ComponentModel.DataAnnotations;

namespace Enums.Types
{
    public enum ColorFlag : int
    {
        Black = 1,
        White = 2,
        Yellow = 4,
        Blue = 8,
        Pink = 16,
        Red = 32,
        Green = 64,
    }


    class TestPen
    {
        public int LengthMm { get; set; }
        public ColorFlag Color { get; set; }
        public Form Form { get; set; }
    }

    class Printer3D : TestPen
    {
        public Printer3D():base()
        {
        }

        public int Temperature { get; set; }
    }

    class Program3D
    {
        void TestPrint()
        {
            var pen = new TestPen();
            pen.Color = ColorFlag.Blue;


            var printer3D = new Printer3D();
            printer3D.Temperature = -44;
        }
    }
}

namespace Enums
{
    enum Form
    {

    }
}