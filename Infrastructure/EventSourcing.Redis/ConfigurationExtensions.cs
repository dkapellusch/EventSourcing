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
                .AddSingleton<IDataStore>(p => p.GetService<RedisDataStore>())
                .AddSingleton<IExpiringDataStore>(p => p.GetService<RedisDataStore>())
                .AddSingleton<RedisLockProvider>()
                .AddSingleton<ILockProvider>(p => p.GetService<RedisLockProvider>())
                .AddSingleton<ISerializer, NewtonJsonSerializer>();
        }
    }
}