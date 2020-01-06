using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using EventSourcing.Contracts;
using Realms;
using Realms.Sync;

namespace EventSourcing.RealmDb
{
    public class RealmStore : IDataStore
    {
        private readonly Realm _realm;

        public RealmStore()
        {
            _realm = Realm.GetInstance(new RealmConfiguration("./realm.db") {SchemaVersion = 1, ShouldDeleteIfMigrationNeeded = true});
        }

        public void Save<T>(Func<T> transaction, string primaryKey) where T : class
        {
            _realm.Write(action: () =>
            {
                var e = transaction();
                if (e is RealmObject r) _realm.Add(r);
            });
        }

        public T Get<T>(string primaryKey) where T : class
        {
            var entity = _realm.Find(typeof(T).Name, primaryKey);
            var e = entity as T;
            return e;
        }

        public void Delete<T>(string primaryKey) where T : class
        {
            _realm.Write(() =>
            {
                var entity = Get<T>(primaryKey);
                if (entity is RealmObject realmEntity)
                    _realm.Remove(realmEntity);
            });
        }

        public IEnumerable<T> Query<T>() where T : class => _realm.All(typeof(T).Name).Select(o => o as T).Where(o => o != null);

        public IEnumerable<T> Query<T>(string startingKey) where T : class => Query<T>();

        public IObservable<T> GetChanges<T>() where T : class
        {
            return _realm.All(typeof(T).Name).Subscribe().Results.OfType<T>().ToObservable();
        }
    }
}