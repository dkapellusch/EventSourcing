using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
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
            Observable.FromAsync(async cancellation =>
                {
                    if (cancellation.IsCancellationRequested) return null;

                    var hasNext = streamReader != null && await streamReader.MoveNext();
                    return hasNext ? streamReader.Current : null;
                })
                .Repeat()
                .TakeWhile(data => data.IsNotNullOrDefault());

        public static IObservable<T> AsObservable<T>(this IAsyncEnumerable<T> asyncStream) where T : class =>
            Observable.FromAsync(async cancellation =>
                {
                    await foreach (var element in asyncStream.WithCancellation(cancellation)) 
                        return element;

                    return null;
                })
                .Repeat();
    }
}