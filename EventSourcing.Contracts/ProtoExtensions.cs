using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Grpc.Core;

namespace EventSourcing.Contracts
{
    public static class ProtoExtensions
    {
        public static IObservable<T> AsObservable<T>(this IAsyncStreamReader<T> streamReader) where T : class =>
            Observable.FromAsync(async _ =>
                {
                    var hasNext = streamReader != null && await streamReader.MoveNext();
                    return hasNext ? streamReader.Current : null;
                })
                .Repeat()
                .TakeWhile(data => data.IsNotNullOrDefault());

        public static bool IsNullOrDefault<T>(this T possiblyNullOrDefaultObject) =>
            possiblyNullOrDefaultObject is null || EqualityComparer<T>.Default.Equals(possiblyNullOrDefaultObject, default);

        public static bool IsNotNullOrDefault<T>(this T possiblyNullOrDefaultObject) => !possiblyNullOrDefaultObject.IsNullOrDefault();

        public static async Task ValidateOrThrowAsync<T>(this IValidator<T> validator, T objectToValidate)
        {
            try
            {
                await validator.ValidateAndThrowAsync(objectToValidate);
            }
            catch (ValidationException validationException)
            {
                var errorMessages = string.Join(" ", validationException.Errors.Select(e => e.ErrorMessage));
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Validation failed."), errorMessages);
            }
        }
    }
}