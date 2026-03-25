using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace Hexblick;

internal interface IApplicationThread
{
    Task RunAsync();
}

internal sealed class ApplicationThread :
    IApplicationThread
{
    private readonly IServiceProvider _serviceProvider;

    public ApplicationThread(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        this._serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public Task RunAsync()
    {
        var thread = new Thread(this.ThreadProc);
        var tcs = new TaskCompletionSource();

        thread.SetApartmentState(ApartmentState.STA);
        thread.UnsafeStart(tcs);

        return tcs.Task;
    }

    private void ThreadProc(object? state)
    {
        Application.Start(_ =>
        {
            var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);

            var app = this._serviceProvider.GetRequiredService<App>();
            app.DispatcherShutdownMode = DispatcherShutdownMode.OnExplicitShutdown;
        });

        ((TaskCompletionSource)state!).SetResult();
    }
}
