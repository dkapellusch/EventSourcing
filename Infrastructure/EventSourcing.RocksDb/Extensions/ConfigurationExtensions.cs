using EventSourcing.Contracts.DataStore;
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
                .AddSingleton<RocksStore>()
                .AddSingleton<IDataStore>(p => p.GetService<RocksStore>())
                .AddSingleton<IChangeTrackingDataStore>(p => p.GetService<RocksStore>())
                .AddSingleton<ISerializer, JsonSerializer>()
                .AddSingleton(typeof(ISerializer<>), typeof(JsonSerializer<>));
    }
}