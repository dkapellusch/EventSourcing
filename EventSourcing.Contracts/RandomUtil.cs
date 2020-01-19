using System;
using System.Collections.Generic;
using System.Linq;

namespace EventSourcing.Contracts
{
    public static class RandomUtil
    {
        private static readonly char[] _vinCharacters =
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L',
            'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X',
            'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        public static T RandomChoice<T>(this IList<T> items)
        {
            var randomIndex = new Random().Next(0, items.Count - 1);
            return items[randomIndex];
        }

        public static double NextDouble(this Random random, double min, double max) => Math.Floor(random.NextDouble() * (max - min) + min);

        public static string GetRandomVin() => string.Join("", Enumerable.Range(0, 17).Select(i => _vinCharacters.RandomChoice()));
    }
}