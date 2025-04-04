namespace CleanArch.Domain.Auxiliary
{
    public class SafeExecuteData
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int StatusCode { get; set; } = 200;
        public Exception? Exception { get; set; }

        public static SafeExecuteData Error(string message, Exception? exception = null)
        {
            return new SafeExecuteData
            {
                Success = false,
                Message = message,
                StatusCode = 500,
                Exception = exception
            };
        }

        public static SafeExecuteData Ok(string message = "Operation completed successfully")
        {
            return new SafeExecuteData
            {
                Success = true,
                Message = message,
                StatusCode = 200
            };
        }

        public static SafeExecuteData NotFound(string message = "Resource not found")
        {
            return new SafeExecuteData
            {
                Success = false,
                Message = message,
                StatusCode = 404
            };
        }

        public static SafeExecuteData BadRequest(string message)
        {
            return new SafeExecuteData
            {
                Success = false,
                Message = message,
                StatusCode = 400
            };
        }

        public static SafeExecuteData Unauthorized(string message = "Unauthorized access")
        {
            return new SafeExecuteData
            {
                Success = false,
                Message = message,
                StatusCode = 401
            };
        }
    }
}