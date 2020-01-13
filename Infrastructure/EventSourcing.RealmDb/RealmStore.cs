using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using Realms;
using Realms.Sync;

namespace EventSourcing.RealmDb
{
    public class RealmStore : IChangeTrackingDataStore
    {
        private readonly Realm _realm;

        public RealmStore() => _realm = Realm.GetInstance(new RealmConfiguration("./realm.db") {SchemaVersion = 1, ShouldDeleteIfMigrationNeeded = true});

        public Task Save<T>(Func<T> transaction, string primaryKey) where T : class
        {
            _realm.Write(() =>
            {
                var e = transaction();
                if (e is RealmObject r) _realm.Add(r);
            });

            return Task.CompletedTask;
        }

        public Task Set<T>(T value, string key) where T : class => Save(() => value, key);

        public Task<T> Get<T>(string key) where T : class
        {
            var entity = _realm.Find(typeof(T).Name, key);
            var e = entity as T;
            return e.ToTask();
        }

        public Task Delete<T>(string key) where T : class
        {
            _realm.Write(() =>
            {
                var entity = Get<T>(key).Result;
                if (entity is RealmObject realmEntity)
                    _realm.Remove(realmEntity);
            });

            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> Query<T>() where T : class => _realm.All(typeof(T).Name).Select(o => o as T).Where(o => o != null).AsEnumerable().ToTask();

        public Task<IEnumerable<T>> Query<T>(string startingKey) where T : class => Query<T>();

        public IObservable<T> GetChanges<T>() where T : class => _realm.All(typeof(T).Name).Subscribe().Results.OfType<T>().ToObservable();
    }
}