using Enums.Types;
using Models.Messages;

namespace Models
{

    public class TestPen
    {
        public TestPen()
        {
            Color = ColorFlag.Green;
        }

        public int LengthMm { get; set; }
        public ColorFlag Color { get; set; }
        public Enums.Form Form { get; set; }
    }

    public class Printer3D : TestPen
    {
        public Printer3D() : base()
        {
            Color = ColorFlag.Yellow;
        }

        public int Temperature { get; set; }
        public Point3DPtinter StartPosition { get; set; }
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

    public class RealClass : IInterface1
    {
        public RealClass(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
