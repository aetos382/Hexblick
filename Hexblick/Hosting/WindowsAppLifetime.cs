using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Hexblick.Hosting;

#pragma warning disable CA1812

internal sealed partial class WindowsAppLifetime :
    IHostLifetime,
    IDisposable
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly HostOptions _hostOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    private readonly TaskCompletionSource<DispatcherQueue> _appStartedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _appStoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private Thread? _appThread;
    private CancellationTokenRegistration _appStoppingRegistration;

    private bool _dispatcherQueueShuttingDown;

    public WindowsAppLifetime(
        IHostApplicationLifetime appLifetime,
        IOptions<HostOptions> hostOptions,
        IServiceProvider serviceProvider,
        ILogger<WindowsAppLifetime> logger)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(hostOptions);
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(logger);

        this._appLifetime = appLifetime;
        this._hostOptions = hostOptions.Value;
        this._serviceProvider = serviceProvider;
        this._logger = logger;
    }

    private sealed record ThreadParams(
        WindowsAppLifetime Self,
        CancellationToken Ct);

    /// <inheritdoc />
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var appStartedTcs = this._appStartedTcs;
        var thread = new Thread(Run)
        {
            IsBackground = false
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.UnsafeStart(new ThreadParams(this, cancellationToken));

        this._appThread = thread;

        return appStartedTcs.Task;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (this._appThread is not { IsAlive: true } thread)
        {
            return;
        }

        var shuttingDown = Volatile.Read(ref this._dispatcherQueueShuttingDown);
        if (!shuttingDown)
        {
            // メッセージ ループが止まるより先にホストの停止要求が来た

#pragma warning disable CA1031
            try
            {
                var dispatcherQueue = await this._appStartedTcs.Task.ConfigureAwait(false);
                dispatcherQueue.EnqueueEventLoopExit();
            }
            catch
            {
                // WaitForStartAsync がキャンセルされた場合を含め、アプリの起動に失敗していても
                // それは StopAsync の責務の失敗を意味しないので、単に return する
                return;
            }
#pragma warning restore CA1031
        }

        await this._appStoppedTcs.Task.WaitAsync(cancellationToken).ConfigureAwait(false);

        thread.Join(this._hostOptions.ShutdownTimeout);
    }

    private static void Run(object? state)
    {
        var (self, cancellationToken) = (ThreadParams)state!;
        var appStartedTcs = self._appStartedTcs;

        if (cancellationToken.IsCancellationRequested)
        {
            appStartedTcs.TrySetCanceled(cancellationToken);
            return;
        }

        var appLifetime = self._appLifetime;

        try
        {
            Application.Start(_ =>
            {
                try
                {
                    var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                    var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                    SynchronizationContext.SetSynchronizationContext(context);

                    dispatcherQueue.ShutdownStarting += self.OnShutdownStarting;

                    self._appStoppingRegistration = appLifetime.ApplicationStopping.UnsafeRegister(
                        self.OnApplicationStopping,
                        dispatcherQueue);

                    var app = self._serviceProvider.GetRequiredService<Application>();
                    if (appStartedTcs.TrySetResult(dispatcherQueue))
                    {
                        self.ApplicationStarted();
                    }
                }
                catch (Exception ex)
                {
                    if (appStartedTcs.TrySetException(ex))
                    {
                        self.ApplicationStartFaulted(ex);
                    }

                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            if (appStartedTcs.TrySetException(ex))
            {
                self.ApplicationStartFaulted(ex);
            }
        }
        finally
        {
            appLifetime.StopApplication();
            self._appStoppedTcs.SetResult();
        }
    }

    private void OnShutdownStarting(DispatcherQueue sender, object args)
    {
        sender.ShutdownStarting -= this.OnShutdownStarting;

        if (!Interlocked.Exchange(ref this._dispatcherQueueShuttingDown, true))
        {
            this.DispatcherQueueShutdownStarted();
        }
    }

    private void OnApplicationStopping(object? state)
    {
        var shuttingDown = Volatile.Read(ref this._dispatcherQueueShuttingDown);
        if (!shuttingDown)
        {
            this.ApplicationStopRequested();

            // 先にホスト側の停止要求が来たのでメッセージループを止める
            ((DispatcherQueue)state!).EnqueueEventLoopExit();
        }
    }

    /// <inheritdoc />
    void IDisposable.Dispose()
    {
        this._appStoppingRegistration.Dispose();
    }

    [LoggerMessage(LogLevel.Information, EventId = 1, EventName = nameof(ApplicationStarted))]
    private partial void ApplicationStarted();

    [LoggerMessage(LogLevel.Critical, EventId = 2, EventName = nameof(ApplicationStartFaulted))]
    private partial void ApplicationStartFaulted(Exception ex);

    [LoggerMessage(LogLevel.Information, EventId = 3, EventName = nameof(DispatcherQueueShutdownStarted))]
    private partial void DispatcherQueueShutdownStarted();

    [LoggerMessage(LogLevel.Information, EventId = 4, EventName = nameof(ApplicationStopRequested))]
    private partial void ApplicationStopRequested();
}
