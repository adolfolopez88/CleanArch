using CleanArch.Domain.Auxiliary;
using CleanArch.Domain.Filters;
using CleanArch.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace CleanArch.Domain.Services
{
    public class SafeExecutorService : ISafeExecutorService
    {
        private readonly ILogger<SafeExecutorService> _logger;

        public SafeExecutorService(ILogger<SafeExecutorService> logger)
        {
            _logger = logger;
        }

        public async Task<SafeExecuteResult<T>> ExecuteAsync<T>(Func<Task<T>> action, string errorMessage = "An error occurred during execution") where T : class
        {
            try
            {
                var result = await action();
                return SafeExecuteResult<T>.Ok(result);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
                return SafeExecuteResult<T>.BadRequest(ex.Message);
            }
            catch (HttpCustomException ex)
            {
                _logger.LogWarning(ex, "HTTP exception: {Message}", ex.Message);
                return new SafeExecuteResult<T>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    Exception = ex
                };
            }
            catch (InternalValidationException ex)
            {
                _logger.LogWarning(ex, "Validation exception: {Message}", ex.Message);
                return new SafeExecuteResult<T>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = 422,
                    Exception = ex
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action: {Message}", ex.Message);
                return SafeExecuteResult<T>.Error(errorMessage, ex);
            }
        }

        public SafeExecuteResult<T> Execute<T>(Func<T> action, string errorMessage = "An error occurred during execution") where T : class
        {
            try
            {
                var result = action();
                return SafeExecuteResult<T>.Ok(result);
            }
            catch (BusinessException ex)
            {
                _logger.LogWarning(ex, "Business exception: {Message}", ex.Message);
                return SafeExecuteResult<T>.BadRequest(ex.Message);
            }
            catch (HttpCustomException ex)
            {
                _logger.LogWarning(ex, "HTTP exception: {Message}", ex.Message);
                return new SafeExecuteResult<T>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = ex.StatusCode,
                    Exception = ex
                };
            }
            catch (InternalValidationException ex)
            {
                _logger.LogWarning(ex, "Validation exception: {Message}", ex.Message);
                return new SafeExecuteResult<T>
                {
                    Success = false,
                    Message = ex.Message,
                    StatusCode = 422,
                    Exception = ex
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing action: {Message}", ex.Message);
                return SafeExecuteResult<T>.Error(errorMessage, ex);
            }
        }
    }
}