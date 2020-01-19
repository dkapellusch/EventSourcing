using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Grpc.Core;

namespace EventSourcing.Contracts.Extensions
{
    public static class Extensions
    {
        public static void Add<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, KeyValuePair<TKey, TValue> keyValuePair)
        {
            dictionary.Add(keyValuePair.Key, keyValuePair.Value);
        }

        public static T UpdateObject<T>(this T destination, T source)
        {
            foreach (var property in typeof(T).GetProperties().Where(p => p.CanWrite))
            {
                var sourceValue = property.GetValue(source, null);
                var destinationValue = property.GetValue(destination, null);

                if (sourceValue is null || destinationValue != null && !string.IsNullOrEmpty(destinationValue.ToString()))
                    continue;

                property.SetValue(destination, sourceValue, null);
            }

            return destination;
        }

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
                throw new RpcException(new Status(StatusCode.InvalidArgument, errorMessages), errorMessages);
            }
        }

        public static string RemoveWhitespace(this string input) =>
            new string(input.ToCharArray()
                .Where(c => !char.IsWhiteSpace(c))
                .ToArray());
    }
}