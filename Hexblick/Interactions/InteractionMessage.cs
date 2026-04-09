using System.Threading;
using System.Threading.Tasks;

namespace Hexblick.Interactions;

internal abstract class InteractionMessage<TResult>
{
    private readonly TaskCompletionSource<TResult> _tcs = new();

    public bool TrySetResult(TResult result)
    {
        return this._tcs.TrySetResult(result);
    }

    public Task<TResult> GetResultAsync(CancellationToken cancellationToken = default)
    {
        return this._tcs.Task.WaitAsync(cancellationToken);
    }
}
