using Firebase.Database;
using MediaProgressTracker.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace MediaProgressTracker.Services.Implementation
{
    public class ExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<ExceptionHandler> _logger;

        public ExceptionHandler(ILogger<ExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async Task<T> HandleAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (FirebaseException fex)
            {
                _logger.LogError(fex, "Firebase error");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                throw;
            }
        }

    }
}
