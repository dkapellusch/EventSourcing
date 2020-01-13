using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;

namespace EventSourcing.Contracts.Extensions
{
    public static class AsyncExtensions
    {
        public static Task<T> ToTask<T>(this T item) => Task.FromResult(item);

        public static async Task<T> WithTimeOut<T>(this Task<T> task, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(timeout);

            try
            {
                return await Task.Run(() => task, cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }

        public static async Task<AsyncServerStreamingCall<T>> WithTimeOut<T>(this AsyncServerStreamingCall<T> task, TimeSpan timeout)
        {
            using var cancellationSource = new CancellationTokenSource();
            cancellationSource.CancelAfter(timeout);

            try
            {
                return await Task.Run(() => task, cancellationSource.Token);
            }
            catch (OperationCanceledException)
            {
                return default;
            }
        }

        public static async Task<List<T>> EnumerateAsync<T>(this IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();
            await foreach (var element in asyncEnumerable) items.Add(element);
            return items;
        }
    }
}