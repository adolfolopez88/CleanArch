using CleanArch.Domain.Auxiliary;

namespace CleanArch.Domain.Interfaces
{
    public interface ISafeExecutorService
    {
        Task<SafeExecuteResult<T>> ExecuteAsync<T>(Func<Task<T>> action, string errorMessage = "An error occurred during execution") where T : class;
        SafeExecuteResult<T> Execute<T>(Func<T> action, string errorMessage = "An error occurred during execution") where T : class;
    }
}