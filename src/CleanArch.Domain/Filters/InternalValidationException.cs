namespace CleanArch.Domain.Filters
{
    public class InternalValidationException : Exception
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; }

        public InternalValidationException(string message, IReadOnlyDictionary<string, string[]> errors) : base(message)
        {
            Errors = errors;
        }
    }
}