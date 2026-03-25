using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;

namespace Hexblick;

internal sealed class WinUIApplicationLifetime :
    IHostLifetime,
    IDisposable
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IWindowManager _windowManager;

    public WinUIApplicationLifetime(
        IHostApplicationLifetime appLifetime,
        IWindowManager windowManager)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(windowManager);

        this._appLifetime = appLifetime;
        this._windowManager = windowManager;
    }

    /// <inheritdoc />
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // TODO マネージリソースをここで解放します
    }

    private void OnFrameworkShutdownCompleted(DispatcherQueue sender, object args)
    {
        sender.FrameworkShutdownCompleted -= this.OnFrameworkShutdownCompleted;

        this._appLifetime.StopApplication();
    }
}
