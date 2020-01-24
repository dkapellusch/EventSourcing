using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.KSQL
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddKsql(this IServiceCollection services, string queryEndpoint) => services
            .AddSingleton(new KsqlClient(new HttpClient {BaseAddress = new Uri(queryEndpoint)}))
            .AddSingleton<KsqlQueryExecutor>()
            .AddSingleton<KsqlStreamCreator>();

        public static IServiceCollection AddKsqlConsumer<TValue>(this IServiceCollection services, KsqlQuery query, Mapper<TValue> mapper) => services
            .AddSingleton(p => new KsqlConsumer<TValue>(p.GetService<KsqlQueryExecutor>(), mapper, query));

        public static IServiceCollection AddKsqlStore<TValue>(this IServiceCollection services, Mapper<TValue> mapper, KsqlQuery changeQuery, KsqlQuery tableQuery) =>
            services.AddSingleton(p => new KsqlStore<TValue>(p.GetService<KsqlQueryExecutor>(), mapper, changeQuery, tableQuery));
    }
}