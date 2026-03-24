using System.Collections.ObjectModel;

using R3;

namespace Hexblick;

internal static class DisposableExtensions
{
    extension<T>(ObservableCollection<T> source)
        where T : IDisposable
    {
        public ObservableCollection<T> AddTo(CompositeDisposable disposables)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(disposables);

            var disposable = Disposable.Create(
                source,
                static source =>
                {
                    foreach (var item in source)
                    {
                        item.Dispose();
                    }
                });

            disposables.Add(disposable);

            return source;
        }
    }
}
