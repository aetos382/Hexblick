using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Hexblick.Hosting;

#pragma warning disable CA1812

internal sealed partial class WindowsAppLifetime<TApplication> :
    IHostLifetime,
    IDisposable
    where TApplication : Application
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource<DispatcherQueue> _appStartedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private Thread? _appThread;
    private CancellationTokenRegistration _appStoppingRegistration;

    private bool _dispatcherQueueShuttingDown;

    public WindowsAppLifetime(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._appLifetime = appLifetime;
        this._serviceProvider = serviceProvider;
    }

    private sealed record ThreadParams(
        TaskCompletionSource<DispatcherQueue> Tcs,
        CancellationToken Ct);

    /// <inheritdoc />
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        var appStartedTcs = this._appStartedTcs;
        var thread = new Thread(this.Run);

        thread.SetApartmentState(ApartmentState.STA);
        thread.UnsafeStart(new ThreadParams(appStartedTcs, cancellationToken));

        this._appThread = thread;

        return appStartedTcs.Task;
    }

    private void Run(object? state)
    {
        var (tcs, cancellationToken) = (ThreadParams)state!;
        if (cancellationToken.IsCancellationRequested)
        {
            tcs.TrySetCanceled(cancellationToken);
            return;
        }

        Application.Start(_ =>
        {
            var appStartedTcs = this._appStartedTcs;

            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
            SynchronizationContext.SetSynchronizationContext(context);

            dispatcherQueue.ShutdownStarting += OnShutdownStarting;
            dispatcherQueue.FrameworkShutdownCompleted += OnFrameworkShutdownCompleted;

            this._appStoppingRegistration = this._appLifetime.ApplicationStopping.UnsafeRegister(
                OnApplicationStopping,
                dispatcherQueue);

            try
            {
                var app = this._serviceProvider.GetRequiredService<TApplication>();
                appStartedTcs.TrySetResult(dispatcherQueue);
            }
            catch (Exception e)
            {
                appStartedTcs.TrySetException(e);
                throw;
            }
        });

        void OnShutdownStarting(DispatcherQueue sender, object args)
        {
            sender.ShutdownStarting -= OnShutdownStarting;

            Volatile.Write(ref this._dispatcherQueueShuttingDown, true);
        }

        void OnFrameworkShutdownCompleted(DispatcherQueue sender, object args)
        {
            sender.FrameworkShutdownCompleted -= OnFrameworkShutdownCompleted;

            // メッセージループが止まったのでホストを止める
            this._appLifetime.StopApplication();
        }

        void OnApplicationStopping(object? state)
        {
            var shuttingDown = Volatile.Read(ref this._dispatcherQueueShuttingDown);
            if (!shuttingDown)
            {
                // 先にホスト側の停止要求が来たのでメッセージループを止める
                ((DispatcherQueue)state!).EnqueueEventLoopExit();
            }
        }
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (this._appThread is not { IsAlive: true } thread)
        {
            return;
        }

        var shuttingDown = Volatile.Read(ref this._dispatcherQueueShuttingDown);
        if (!shuttingDown)
        {
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

        var task = Task.Factory.StartNew(
            static state => ((Thread)state!).Join(),
            thread,
            cancellationToken,
            TaskCreationOptions.None,
            TaskScheduler.Default);

        await task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._appStoppingRegistration.Dispose();
    }
}
