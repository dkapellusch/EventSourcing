using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RocksStore : IChangeTrackingDataStore, IDataStore, IQueryableDataStore
    {
        private readonly RockCollection _db;

        public RocksStore(RockCollection db) => _db = db;

        public Task Set<T>(T value, string key)
        {
            _db.Add(key, value);
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key) => _db.Get<string, T>(key).ToTask();

        public IObservable<T> GetChanges<T>() => _db.ChangedDataCaptureStream.OfType<DataChangedEvent<string, T>>().Select(dc => dc.Data.value);

        public Task Delete<T>(string key)
        {
            _db.Delete<string, T>(key);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> Query<T>() =>
            _db.GetItems<string, T>().Select(kv => kv.value).ToTask();

        public Task<IEnumerable<T>> Query<T>(string startingKey) =>
            _db.GetItems<string, T>(startingKey).Select(kv => kv.value).ToTask();
    }
}