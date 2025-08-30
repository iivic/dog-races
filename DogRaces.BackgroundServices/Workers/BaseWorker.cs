using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DogRaces.BackgroundServices.Workers;

/// <summary>
/// Abstract base class for background workers with consistent error handling,
/// logging, and service scope management.
/// </summary>
public abstract class BaseWorker(
    ILogger<BaseWorker> logger, 
    IServiceProvider serviceProvider, 
    int executeEverySeconds) : BackgroundService
{
    protected ILogger<BaseWorker> Logger { get; } = logger;
    protected IServiceProvider ServiceProvider { get; } = serviceProvider;
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{Name} started", GetType().Name);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = ServiceProvider.CreateScope();
                await Execute(scope, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in {Name}", GetType().Name);
            }
            finally
            {
                await Task.Delay(TimeSpan.FromSeconds(executeEverySeconds), stoppingToken);
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{Name} stopped", GetType().Name);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Execute the worker's core logic. Called every executeEverySeconds interval.
    /// </summary>
    /// <param name="scope">Service scope for dependency injection</param>
    /// <param name="cancellationToken">Cancellation token</param>
    protected abstract Task Execute(IServiceScope scope, CancellationToken cancellationToken);
}
