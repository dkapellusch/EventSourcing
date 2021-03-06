using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Newtonsoft.Json.Linq;

namespace EventSourcing.KSQL
{
    public static class KsqlExtensions
    {
        public static TProperty GetValue<TProperty>(this IDictionary<string, dynamic> columns, string columnName)
        {
            if (!columns.TryGetValue(columnName.ToUpperInvariant(), out var column)) return default;

            var columnVal = GetColumnValue<TProperty>(column);
            return (TProperty) columnVal;
        }

        public static TProperty GetValue<TSourceType, TProperty>(this IDictionary<string, dynamic> columns, Expression<Func<TSourceType, TProperty>> expression)
        {
            var propertyName = ((MemberExpression) expression.Body).Member.Name;
            if (!columns.TryGetValue(propertyName.ToUpperInvariant(), out var column)) return default;

            var columnVal = GetColumnValue<TProperty>(column);
            return (TProperty) columnVal;
        }

        public static TProperty GetValue<TSourceType, TProperty>(this IDictionary<string, dynamic> columns, Expression<Func<TSourceType, TProperty>> expression, Func<string, TProperty> converter)
        {
            var propertyName = ((MemberExpression) expression.Body).Member.Name;
            if (!columns.TryGetValue(propertyName.ToUpperInvariant(), out var column)) return default;

            var columnVal = GetColumnValue<TProperty>(column);
            var converted = converter(columnVal.ToString());
            return converted;
        }

        private static dynamic GetColumnValue<T>(dynamic value, T defaultValue = default)
        {
            if (value is null) return defaultValue;

            Type valueType = value.GetType();
            if (valueType == typeof(JArray)) value = value.Last;
            return value;
        }
    }
}