using System;
using System.Reactive.Linq;
using Grpc.Core;

namespace EventSourcing.Contracts.Extensions
{
    public static class ObservableExtensions
    {
        public static IObservable<T> RetryAfter<T>(this IObservable<T> source, int retryCount, int delayMs, Action<Exception> onError)
        {
            return source.RetryAfter(retryCount, delayMs)
                .Catch(new Func<Exception, IObservable<T>>(error =>
                {
                    onError(error);
                    return Observable.Empty<T>();
                }));
        }

        public static IObservable<T> RetryAfter<T>(this IObservable<T> source, int retryCount, int delayMs)
        {
            var attempt = 0;
            return Observable.Defer(() =>
                    ++attempt == 1
                        ? source
                        : source.DelaySubscription(TimeSpan.FromMilliseconds(delayMs)))
                .Retry(retryCount);
        }

        public static IObservable<T> AsObservable<T>(this IAsyncStreamReader<T> streamReader) where T : class =>
            Observable.FromAsync(async _ =>
                {
                    var hasNext = streamReader != null && await streamReader.MoveNext();
                    return hasNext ? streamReader.Current : null;
                })
                .Repeat()
                .TakeWhile(data => data.IsNotNullOrDefault());
    }
}