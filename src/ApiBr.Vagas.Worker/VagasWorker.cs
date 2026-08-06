namespace ApiBr.Vagas.Worker;

public sealed class VagasWorker(ILogger<VagasWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // AMQP consumption loop lands in Phase 4 — this placeholder just proves the host stays alive.
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Worker heartbeat at {Time}", DateTimeOffset.Now);
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
