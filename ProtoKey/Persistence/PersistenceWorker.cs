using System.Threading.Channels;

namespace ProtoKey.Persistence;

internal sealed class PersistenceWorker(
    Channel<PersistedCommand> commands,
    IWebHostEnvironment environment,
    ILogger<PersistenceWorker> logger) : BackgroundService
{
    private readonly List<PersistedCommand> pendingCommands = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
                DrainCommands();
                await FlushAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to persist commands.");
                FailPendingCommands(exception);
            }
        }

        DrainCommands();
        await FlushAsync(CancellationToken.None);
    }

    private void DrainCommands()
    {
        while (commands.Reader.TryRead(out PersistedCommand? command))
        {
            pendingCommands.Add(command);
        }
    }

    private async Task FlushAsync(CancellationToken cancellationToken)
    {
        if (pendingCommands.Count == 0)
        {
            return;
        }

        string path = PersistencePaths.DataFile(environment.ContentRootPath);
        await File.AppendAllLinesAsync(
            path,
            pendingCommands.Select(command => command.ToDataLine()),
            cancellationToken);

        pendingCommands.ForEach(command => command.Completion?.TrySetResult());

        pendingCommands.Clear();
    }

    private void FailPendingCommands(Exception exception)
    {
        pendingCommands.ForEach(command => command.Completion?.TrySetException(exception));
        pendingCommands.Clear();
    }
}
