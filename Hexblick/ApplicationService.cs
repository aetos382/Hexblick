using Microsoft.Extensions.Hosting;

namespace Hexblick;

internal sealed class ApplicationService :
    BackgroundService
{
    private readonly IApplicationThread _appThread;

    private Task? _executeTask;
    private CancellationTokenSource? _stoppingCts;

    public ApplicationService(
        IApplicationThread appThread)
    {
        ArgumentNullException.ThrowIfNull(appThread);
        this._appThread = appThread;
    }

    /// <inheritdoc />
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
    }

    /// <inheritdoc />
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        this._stoppingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _ = this._appThread.RunAsync();

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
