namespace MediaProgressTracker.Services.Abstract
{
    public interface IExceptionHandler
    {
        Task<T> HandleAsync<T>(Func<Task<T>> action);
    }
}
