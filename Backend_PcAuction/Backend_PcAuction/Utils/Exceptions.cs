namespace Backend_PcAuction.Utils
{
    public class CustomException : Exception
    {
        public int ErrorCode { get; }

        public CustomException(int errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class NoPartsException : CustomException
    {
        public NoPartsException(string message) : base(1, message) { }
    }

    public class NoCompatibilityException : CustomException
    {
        public NoCompatibilityException(string message) : base(2, message) { }
    }

    public class NoBudgetException : CustomException
    {
        public NoBudgetException(string message) : base(3, message) { }
    }
}
