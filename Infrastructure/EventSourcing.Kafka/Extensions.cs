using System.Linq;

namespace EventSourcing.Kafka
{
    public static class Extensions
    {
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
    }
}