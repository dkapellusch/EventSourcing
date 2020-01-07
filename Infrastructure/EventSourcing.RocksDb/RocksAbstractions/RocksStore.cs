using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using EventSourcing.Contracts;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RocksStore : IDataStore
    {
        private readonly RockCollection _db;

        public RocksStore(RockCollection db) => _db = db;

        public void Save<T>(Func<T> transaction, string primaryKey) where T : class
        {
            var entity = transaction();
            _db.Add(primaryKey, entity);
        }

        public void Delete<T>(string primaryKey) where T : class => _db.Delete<string, T>(primaryKey);

        public T Get<T>(string primaryKey) where T : class => _db.Get<string, T>(primaryKey);

        public IEnumerable<T> Query<T>() where T : class =>
            _db.GetItems<string, T>().Select(kv => kv.value);

        public IEnumerable<T> Query<T>(string startingKey) where T : class =>
            _db.GetItems<string, T>(startingKey).Select(kv => kv.value);

        public IObservable<T> GetChanges<T>() where T : class => _db.ChangedDataCaptureStream.OfType<DataChangedEvent<string, T>>().Select(dc => dc.Data.value);
    }
}