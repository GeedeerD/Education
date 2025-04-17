using Enums.Types;

namespace Models
{

    public class TestPen
    {
        public int LengthMm { get; set; }
        public ColorFlag Color { get; set; }
        public Enums.Form Form { get; set; }
    }

    public class Printer3D : TestPen
    {
        public Printer3D() : base()
        {
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
}
