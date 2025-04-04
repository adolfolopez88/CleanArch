namespace CleanArch.Domain.Filters
{
    public class HttpCustomException : Exception
    {
        public int StatusCode { get; }
        public string? Detail { get; }

        public HttpCustomException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpCustomException(int statusCode, string message, string detail) : base(message)
        {
            StatusCode = statusCode;
            Detail = detail;
        }

        public HttpCustomException(int statusCode, string message, Exception innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}