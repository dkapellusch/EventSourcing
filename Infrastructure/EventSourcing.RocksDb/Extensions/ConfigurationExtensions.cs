using EventSourcing.Contracts;
using EventSourcing.Contracts.Serialization;
using EventSourcing.RocksDb.RocksAbstractions;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.RocksDb.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddRocksDb(this IServiceCollection services, string pathToDb) =>
            services.AddSingleton(new RocksDatabase(pathToDb))
                .AddSingleton<RockCollection>()
                .AddSingleton<IDataStore, RocksStore>()
                .AddSingleton<ISerializer, JsonSerializer>()
                .AddSingleton(typeof(ISerializer<>), typeof(JsonSerializer<>));
    }
}