using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.KSQL
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddKsql(this IServiceCollection services, string queryEndpoint) => services
            .AddSingleton(new KsqlClient(new HttpClient {BaseAddress = new Uri(queryEndpoint)}))
            .AddSingleton<KsqlQueryExecutor>();
    }
}