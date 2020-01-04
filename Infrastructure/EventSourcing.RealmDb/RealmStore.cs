using System;
using System.Collections.Generic;
using System.Linq;
using EventSourcing.Contracts;
using Realms;

namespace EventSourcing.RealmDb
{
    public class RealmStore : IDataStore
    {
        private readonly Realm _realm;

        public RealmStore() => _realm = Realm.GetInstance(new RealmConfiguration("./realm.db") {SchemaVersion = 1, ShouldDeleteIfMigrationNeeded = true});


        public void Save<T>(Func<T> transaction) where T : class, IEntity
        {
            _realm.Write(() =>
            {
                if (transaction() is RealmObject realmEntity)
                    _realm.Add(realmEntity);
            });
        }

        public T Get<T>(string primaryKey) where T : class, IEntity
        {
            var entity = _realm.Find(typeof(T).Name, primaryKey);

            var e = entity as T;
            if (e != null) e.DataStore = this;
            return e;
        }

        public void Delete<T>(string primaryKey) where T : class, IEntity
        {
            _realm.Write(() =>
            {
                var entity = Get<T>(primaryKey);
                if (entity is RealmObject realmEntity)
                    _realm.Remove(realmEntity);
            });
        }

        public IEnumerable<T> Query<T>() where T : class, IEntity => _realm.All<RealmObject>().Select(o => o as T).Where(o => o != null);

        public IEnumerable<T> Query<T>(string startingKey) where T : class, IEntity => Query<T>().Where(e => e.PrimaryKey.Contains(startingKey));
    }
}