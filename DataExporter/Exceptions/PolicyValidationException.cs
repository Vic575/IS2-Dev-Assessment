namespace DataExporter.Exceptions
{
    public class PolicyValidationException : Exception
    {
        public string ValidationError { get; }

        public PolicyValidationException(string message) : base(message)
        {
            ValidationError = message;
        }
    }
}
