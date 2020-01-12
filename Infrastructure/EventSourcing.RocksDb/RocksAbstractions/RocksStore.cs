using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RocksStore : IChangeTrackingDataStore, ITransactionalDataStore
    {
        private readonly RockCollection _db;

        public RocksStore(RockCollection db) => _db = db;

        public Task Save<T>(Func<T> transaction, string primaryKey) where T : class
        {
            var entity = transaction();
            _db.Add(primaryKey, entity);
            return Task.CompletedTask;
        }

        public Task Delete<T>(string key) where T : class
        {
            _db.Delete<string, T>(key);
            return Task.CompletedTask;
        }

        public Task Set<T>(T value, string key) where T : class
        {
            _db.Add(key, value);
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key) where T : class => _db.Get<string, T>(key).ToTask();

        public Task<IEnumerable<T>> Query<T>() where T : class =>
            _db.GetItems<string, T>().Select(kv => kv.value).ToTask();

        public Task<IEnumerable<T>> Query<T>(string startingKey) where T : class =>
            _db.GetItems<string, T>(startingKey).Select(kv => kv.value).ToTask();

        public IObservable<T> GetChanges<T>() where T : class => _db.ChangedDataCaptureStream.OfType<DataChangedEvent<string, T>>().Select(dc => dc.Data.value);
    }
}