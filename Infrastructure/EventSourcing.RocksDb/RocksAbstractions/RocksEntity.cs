using System.Collections.Generic;
using System.Linq;
using System.Text;
using EventSourcing.Contracts;
using EventSourcing.RocksDb.Extensions;
using EventSourcing.RocksDb.Serialization;
using RocksDbSharp;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RocksStore : IDataStore
    {
        private readonly RocksDatabase _db;
        private readonly ISerializer _serializer;

        public RocksStore(RocksDatabase db, ISerializer serializer)
        {
            _db = db;
            _serializer = serializer;
        }

        private RocksDbSharp.RocksDb RocksDb => _db.RocksDb;

        public void Save<T>(T entity) where T : class, IEntity => RocksDb.Put(entity.PrimaryKey, _serializer.Serialize(entity), GetColumnFamily<T>());

        public void Delete<T>(string primaryKey) where T : class, IEntity => RocksDb.Remove(primaryKey, GetColumnFamily<T>());

        public T Get<T>(string primaryKey) where T :  class, IEntity
        {
            var entity = _serializer.Deserialize<T>(RocksDb.Get(Encoding.UTF8.GetBytes(primaryKey), GetColumnFamily<T>()));
            entity.Attach(this);
            return entity;
        }

        public IEnumerable<T> Query<T>() where T :  class, IEntity
        {
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<T>(), iteratorOptions)
                .SeekToFirst()
                .GetEnumerable()
                .Select(kv =>
                {
                    var entity = _serializer.Deserialize<T>(kv.value);
                    entity.Attach(this);
                    return entity;
                });
        }

        public IEnumerable<T> Query<T>(string startingKey) where T : class, IEntity
        {
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<T>(), iteratorOptions)
                .Seek(startingKey)
                .GetEnumerable()
                .Select(kv =>
                {
                    var entity = _serializer.Deserialize<T>(kv.value);
                    entity.Attach(this);
                    return entity;
                });
        }

        private ColumnFamilyHandle GetColumnFamily<T>() where T : class, IEntity => _db.GetOrCreateColumnFamily(typeof(T).Name);
    }
}