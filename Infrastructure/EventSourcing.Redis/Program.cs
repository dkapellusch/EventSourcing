using System;
using System.Threading.Tasks;
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

    public class Program
    {
        public static async Task Main()
        {
            var connectionString = "localhost:6379";
            var mux = await ConnectionMultiplexer.ConnectAsync(connectionString);
            var db = mux.GetDatabase();
            var redisStore = new RedisDataStore(db, new NewtonJsonSerializer());

            await redisStore.Set("1", "2", TimeSpan.FromSeconds(1));
            redisStore.ExpiredKeys.Subscribe(k => { Console.WriteLine($"{k} expired..."); });
            var val = await redisStore.Get<string>("2");


            await Task.Delay(10_000);
            Console.WriteLine(val);
        }
    }
}