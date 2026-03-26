using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Hexblick;

internal sealed partial class WinAppLifetime<TApplication> :
    IHostLifetime,
    IDisposable
    where TApplication : Application
{
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly TaskCompletionSource _appStartedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource _appStoppedTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);

    private CancellationTokenRegistration _startCancellationRegistration;
    private CancellationTokenRegistration _appStoppingRegistration;
    private CancellationTokenRegistration _appStoppedRegistration;

    private bool _dispatcherQueueShuttingDown;

    public WinAppLifetime(
        IHostApplicationLifetime appLifetime,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(appLifetime);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._appLifetime = appLifetime;
        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        var appStartedTcs = this._appStartedTcs;

        this._startCancellationRegistration = cancellationToken.UnsafeRegister(
            static (state, token) => ((TaskCompletionSource)state!).TrySetCanceled(token),
            appStartedTcs);

        var thread = new Thread(this.Run);

        thread.SetApartmentState(ApartmentState.STA);
        thread.UnsafeStart(cancellationToken);

        return appStartedTcs.Task;
    }

    private void Run(object? state)
    {
        try
        {
            ((CancellationToken)state!).ThrowIfCancellationRequested();

            Application.Start(_ =>
            {
                var appStartedTcs = this._appStartedTcs;

                var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

                var context = new DispatcherQueueSynchronizationContext(dispatcherQueue);
                SynchronizationContext.SetSynchronizationContext(context);

                try
                {
                    dispatcherQueue.ShutdownStarting += OnShutdownStarting;
                    dispatcherQueue.FrameworkShutdownCompleted += OnFrameworkShutdownCompleted;

                    this._appStoppingRegistration = this._appLifetime.ApplicationStopping.UnsafeRegister(
                        OnApplicationStopping,
                        dispatcherQueue);

                    var app = this._serviceProvider.GetRequiredService<TApplication>();

                    appStartedTcs.TrySetResult();
                }
                catch (Exception e)
                {
                    appStartedTcs.TrySetException(e);
                    throw;
                }
                finally
                {
                    // ReSharper disable once AccessToDisposedClosure
                    this._startCancellationRegistration.Dispose();
                }
            });
        }
        finally
        {
            this._appStoppedTcs.TrySetResult();
            this._startCancellationRegistration.Dispose();
        }

        void OnShutdownStarting(DispatcherQueue sender, object args)
        {
            sender.ShutdownStarting -= OnShutdownStarting;

            Volatile.Write(ref this._dispatcherQueueShuttingDown, true);
        }

        void OnFrameworkShutdownCompleted(DispatcherQueue sender, object args)
        {
            sender.FrameworkShutdownCompleted -= OnFrameworkShutdownCompleted;

            this._appLifetime.StopApplication();
        }

        void OnApplicationStopping(object? state)
        {
            var shuttingDown = Volatile.Read(ref this._dispatcherQueueShuttingDown);

            if (!shuttingDown)
            {
                ((DispatcherQueue)state!).EnqueueEventLoopExit();
            }
        }
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        var appStoppedTcs = this._appStoppedTcs;

        if (!appStoppedTcs.Task.IsCompleted)
        {
            this._appStoppedRegistration = cancellationToken.UnsafeRegister(
                static (state, token) => ((TaskCompletionSource)state!).TrySetCanceled(token),
                appStoppedTcs);
        }

        return appStoppedTcs.Task;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this._startCancellationRegistration.Dispose();
        this._appStoppingRegistration.Dispose();
        this._appStoppedRegistration.Dispose();
    }
}
