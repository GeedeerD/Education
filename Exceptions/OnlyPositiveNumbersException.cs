namespace Exceptions
{
    public class OnlyPositiveNumbersException : Exception
    {
        public string invalidNumber = string.Empty;
        public OnlyPositiveNumbersException(string message, string value) : base(message)
        {
            invalidNumber = value;
            Data.Add( "item1", "test" );
        }
    }
}
