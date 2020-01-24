using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RocksStore<T>
    {
        private readonly RocksStore _rocksStore;

        public RocksStore(RocksStore rocksStore) => _rocksStore = rocksStore;

        public async Task<T> Get(string key) => await _rocksStore.Get<T>(key);

        public async Task Set(T value, string key) => await _rocksStore.Set(value, key);

        public async Task<IEnumerable<T>> Query() => await _rocksStore.Query<T>();

        public async Task<IEnumerable<T>> Query(string startingKey) => await _rocksStore.Query<T>(startingKey);

        public async Task Delete(string key) => await _rocksStore.Delete<T>(key);

        public IObservable<T> GetChanges() => _rocksStore.GetChanges<T>();
    }

    public class RocksStore
    {
        private readonly RockCollection _db;

        public RocksStore(RockCollection db) => _db = db;

        public Task Set<T>(T value, string key)
        {
            _db.Add(key, value);
            return Task.CompletedTask;
        }

        public Task<T> Get<T>(string key) => _db.Get<string, T>(key).ToTask();

        public IObservable<T> GetChanges<T>() => _db.ChangedDataCaptureStream
            .OfType<DataChangedEvent<string, T>>()
            .Select(dc => dc.Data.value);

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