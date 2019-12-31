using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using EventSourcing.RocksDb.Extensions;
using EventSourcing.RocksDb.Serialization;
using RocksDbSharp;

namespace EventSourcing.RocksDb.RocksAbstractions
{
    public abstract class RocksEntity : INotifyPropertyChanged
    {
        internal static Rocky Instance;

        protected RocksEntity()
        {
            PropertyChanged += (sender, args) =>
            {
                Updated = true;
                if (AutoSave) Save();
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract string PrimaryKey { get; }

        public virtual bool AutoSave => false;

        internal bool Updated { get; private set; }

        public static void ConnectDatabase(Rocky rocky) => Instance = rocky;

        public void Save()
        {
            if (Instance is null) throw new InvalidOperationException("Not connected to a rocks instance, call ConnectDatabase with an instance before saving.");
            Instance.Save(this, GetType());
        }

        public void Delete()
        {
            if (Instance is null) throw new InvalidOperationException("Not connected to a rocks instance, call ConnectDatabase with an instance before saving.");
            Instance.Delete(this);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Rocky
    {
        private readonly RocksDatabase _db;
        private readonly ISerializer _serializer;

        public Rocky(RocksDatabase db, ISerializer serializer)
        {
            _db = db;
            _serializer = serializer;
        }

        private RocksDbSharp.RocksDb RocksDb => _db.RocksDb;

        public void Save<T>(T entity) where T : RocksEntity
        {
            if (entity.Updated)
                RocksDb.Put(entity.PrimaryKey, _serializer.Serialize(entity), GetColumnFamily<T>());
        }

        public void Save<T>(T entity, Type type) where T : RocksEntity
        {
            if (entity.Updated)
                RocksDb.Put(entity.PrimaryKey, _serializer.Serialize(entity), GetColumnFamily(type));
        }

        public void Delete<T>(T entity) where T : RocksEntity => RocksDb.Remove(entity.PrimaryKey, GetColumnFamily<T>());

        public void Delete<T>(string primaryKey) where T : RocksEntity => RocksDb.Remove(primaryKey, GetColumnFamily<T>());
        public T Get<T>(string primaryKey) where T : RocksEntity => _serializer.Deserialize<T>(RocksDb.Get(Encoding.UTF8.GetBytes(primaryKey), GetColumnFamily<T>()));

        public IEnumerable<T> Query<T>() where T : RocksEntity
        {
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<T>(), iteratorOptions)
                .SeekToFirst()
                .GetEnumerable()
                .Select(kv => _serializer.Deserialize<T>(kv.value));
        }

        public IEnumerable<T> Query<T>(string startingKey) where T : RocksEntity
        {
            var iteratorOptions = new ReadOptions();

            return RocksDb
                .NewIterator(GetColumnFamily<T>(), iteratorOptions)
                .Seek(startingKey)
                .GetEnumerable()
                .Select(kv => _serializer.Deserialize<T>(kv.value));
        }

        private ColumnFamilyHandle GetColumnFamily<T>() => _db.GetOrCreateColumnFamily(typeof(T).Name);
        private ColumnFamilyHandle GetColumnFamily(Type type) => _db.GetOrCreateColumnFamily(type.Name);
    }
}