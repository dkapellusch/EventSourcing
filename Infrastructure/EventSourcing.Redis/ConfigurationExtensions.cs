using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Serialization;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace EventSourcing.Redis
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddRedisDataStore(this IServiceCollection services, string connectionString)
        {
            return services
                .AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(connectionString))
                .AddSingleton(p => p.GetService<IConnectionMultiplexer>().GetDatabase())
                .AddSingleton<IDatabaseAsync>(p => p.GetService<IDatabase>())
                .AddSingleton<RedisDataStore>()
                .AddSingleton<IExpiringDataStore, RedisDataStore>()
                .AddSingleton<IDataStore, RedisDataStore>()
                .AddSingleton(typeof(ISerializer<>), typeof(JsonMessageSerializer<>))
                .AddSingleton<ISerializer, NewtonJsonSerializer>();
        }
    }
}