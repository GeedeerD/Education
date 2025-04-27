using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numbers;

namespace Encapsulation
{
    public class Realisation
    {
        public ArrayUserException _userException;

        public Realisation()
        {
            int numberA;
            _userException = new ArrayUserException();
            _userException.CountOfElements = 5;

            var battery1 = new C32Battery();
            Console.WriteLine("Type:{0}, voltage:{1}", battery1.FormTypePublic, battery1.VoltagePublic);

            var battery2 = new AaBattery();
            Console.WriteLine("Type:{0}, voltage:{1}", battery2.FormTypePublic, battery2.VoltagePublic);
        }
    }

    public abstract class Battery
    {
        protected double Voltage { get; set; }
        protected FormType FormType { get; set; }

        public double VoltagePublic => Voltage;
        public FormType FormTypePublic => FormType;
    }

    public enum FormType : int
    {
        unknown = 0,
        C32 = 1,
        AA = 2,
    }

    public class C32Battery : Battery
    {
        public C32Battery()
        {
            Voltage = 3.65;
            FormType = FormType.C32;
        }
    }

    public class AaBattery : Battery
    {
        public AaBattery()
        {
            Voltage = 1.56;
            FormType = FormType.AA;
        }
    }
}
