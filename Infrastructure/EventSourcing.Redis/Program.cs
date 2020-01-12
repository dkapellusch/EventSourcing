using System;
using System.Threading.Tasks;
using EventSourcing.Contracts.Serialization;
using StackExchange.Redis;

namespace EventSourcing.Redis
{
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