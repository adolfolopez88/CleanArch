namespace CleanArch.Domain.Auxiliary
{
    public class SafeExecuteResult<T> where T : class
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public int StatusCode { get; set; } = 200;
        public Exception? Exception { get; set; }

        public static SafeExecuteResult<T> Error(string message, Exception? exception = null)
        {
            return new SafeExecuteResult<T>
            {
                Success = false,
                Message = message,
                StatusCode = 500,
                Exception = exception
            };
        }

        public static SafeExecuteResult<T> Ok(T data, string message = "Operation completed successfully")
        {
            return new SafeExecuteResult<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200
            };
        }

        public static SafeExecuteResult<T> NotFound(string message = "Resource not found")
        {
            return new SafeExecuteResult<T>
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        public static SafeExecuteResult<T> BadRequest(string message)
        {
            return new SafeExecuteResult<T>
            {
                Success = false,
                Message = message,
                StatusCode = 400
            };
        }

        public static SafeExecuteResult<T> Unauthorized(string message = "Unauthorized access")
        {
            return new SafeExecuteResult<T>
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }
    }
}