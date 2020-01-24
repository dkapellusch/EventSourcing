using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using EventSourcing.Contracts.Serialization;
using EventSourcing.RocksDb.Extensions;
using RocksDbSharp;
using static System.Diagnostics.Stopwatch;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public class RockCollection<TKey, TValue>
    {
        private readonly RockCollection _rockCollection;

        public RockCollection(RockCollection rockCollection) => _rockCollection = rockCollection;

        public IObservable<DataChangedEvent<TKey, TValue>> ChangedDataCaptureStream => _rockCollection.ChangedDataCaptureStream.OfType<DataChangedEvent<TKey, TValue>>();

        public void Add(TKey key, TValue value) => _rockCollection.Add(key, value);

        public TValue Get(TKey key) => _rockCollection.Get<TKey, TValue>(key);

        public void Delete(TKey key) => _rockCollection.Delete<TKey, TValue>(key);

        public IEnumerable<(TKey key, TValue value)> GetItems() => _rockCollection.GetItems<TKey, TValue>();

        public IEnumerable<(TKey key, TValue value)> GetItems(TKey key) => _rockCollection.GetItems<TKey, TValue>(key);
    }

    public class RockCollection
    {
        private readonly RocksDatabase _db;
        private readonly ISerializer _serializer;
        private readonly Subject<object> _subject;
        private IObservable<object> _stream;

        public RockCollection(RocksDatabase db, ISerializer serializer)
        {
            _db = db;
            _serializer = serializer;
            _subject = new Subject<object>();
        }

        private RocksDbSharp.RocksDb RocksDb => _db.RocksDb;

        public IObservable<object> ChangedDataCaptureStream => _stream ??= _subject.AsObservable();

        public void Add<TKey, TValue>(TKey key, TValue itemToAdd)
        {
            var dataEvent = new DataChangedEvent<TKey, TValue>(Operation.DataUpdated, (key, itemToAdd));
            RocksDb.Put(_serializer.Serialize(key), _serializer.Serialize(itemToAdd), GetColumnFamily<TValue>());
            _subject.OnNext(dataEvent);
        }

        public TValue Get<TKey, TValue>(TKey key)
        {
            var item = RocksDb.Get(_serializer.Serialize(key), GetColumnFamily<TValue>());
            return _serializer.Deserialize<TValue>(item);
        }

        public void Delete<TKey, TValue>(TKey key)
        {
            var dataEvent = new DataChangedEvent<TKey, TValue>(Operation.DataDeleted, (key, default));
            RocksDb.Remove(_serializer.Serialize(key), GetColumnFamily<TValue>());
            _subject.OnNext(dataEvent);
        }

        public IEnumerable<(TKey key, TValue value)> GetItems<TKey, TValue>(TKey key, Func<TKey, TValue, bool> condition)
        {
            var startingKey = _serializer.Serialize(key);
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<TValue>(), iteratorOptions)
                .Seek(startingKey)
                .GetEnumerable((serializedKey, serializedValue) => condition(_serializer.Deserialize<TKey>(serializedKey), _serializer.Deserialize<TValue>(serializedValue)))
                .Select(kv => (_serializer.Deserialize<TKey>(kv.key), _serializer.Deserialize<TValue>(kv.value)));
        }

        public IEnumerable<(TKey key, TValue value)> GetItems<TKey, TValue>(TKey key)
        {
            var startingKey = _serializer.Serialize(key);
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<TValue>(), iteratorOptions)
                .Seek(startingKey)
                .GetEnumerable()
                .Select(kv => (_serializer.Deserialize<TKey>(kv.key), _serializer.Deserialize<TValue>(kv.value)));
        }

        public IEnumerable<(TKey key, TValue value)> GetItems<TKey, TValue>()
        {
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<TValue>(), iteratorOptions)
                .SeekToFirst()
                .GetEnumerable()
                .Select(kv => (_serializer.Deserialize<TKey>(kv.key), _serializer.Deserialize<TValue>(kv.value)));
        }

        private ColumnFamilyHandle GetColumnFamily<T>() => _db.GetOrCreateColumnFamily(typeof(T).Name);

        private ColumnFamilyHandle GetAuditColumnFamily<T>() => _db.GetOrCreateColumnFamily($"Audit/{typeof(T).Name}");

        private bool WriteBatch(WriteBatch batch)
        {
            try
            {
                batch.SetSavePoint();
                RocksDb.Write(batch);
                return true;
            }
            catch
            {
                batch.RollbackToSavePoint();
                return false;
            }
        }
    }
}