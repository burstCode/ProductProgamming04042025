using System.Collections.Concurrent;

namespace ProductProgamming04042025.Pages.Helpers
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<IServiceProvider, CancellationToken, Task> task);
        Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }

    // BackgroundTaskQueue.cs
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private readonly ConcurrentQueue<Func<IServiceProvider, CancellationToken, Task>> _tasks = new();
        private readonly SemaphoreSlim _signal = new(0);

        public void Enqueue(Func<IServiceProvider, CancellationToken, Task> task)
        {
            _tasks.Enqueue(task);
            _signal.Release();
        }

        public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _tasks.TryDequeue(out var task);
            return task;
        }
    }

    // BackgroundTaskService.cs
    public class BackgroundTaskService : BackgroundService
    {
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BackgroundTaskService> _logger;

        public BackgroundTaskService(
            IBackgroundTaskQueue taskQueue,
            IServiceProvider serviceProvider,
            ILogger<BackgroundTaskService> logger)
        {
            _taskQueue = taskQueue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var task = await _taskQueue.DequeueAsync(stoppingToken);
                    await task(_serviceProvider, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в фоновой задаче");
                }
            }
        }
    }
}