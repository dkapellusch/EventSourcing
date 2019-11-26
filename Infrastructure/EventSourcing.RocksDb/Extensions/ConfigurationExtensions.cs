using EventSourcing.RocksDb.RocksAbstractions;
using EventSourcing.RocksDb.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.RocksDb.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddRocksDb(this IServiceCollection services, string pathToDb) =>
            services.AddSingleton(new RocksDatabase(pathToDb))
                .AddSingleton<RocksStore>()
                .AddSingleton<ISerializer, JsonSerializer>()
                .AddSingleton(typeof(ISerializer<>), typeof(JsonSerializer<>));
    }
}