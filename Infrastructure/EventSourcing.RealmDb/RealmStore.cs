using System.Linq;
using System.Threading.Tasks;
using Realms;

namespace EventSourcing.RealmDb
{
    public class RealmStore 
    {
        private readonly Realm _realm;

        public RealmStore() => _realm = Realm.GetInstance(new RealmConfiguration("./realm.db") {SchemaVersion = 5u});

        public async Task AddAsync<T>(T item) where T : RealmObject => await _realm.WriteAsync(r => r.Add(item));

        public async Task AddAsync(params RealmObject[] items) =>
            await _realm.WriteAsync(r =>
            {
                foreach (var item in items)
                    r.Add(item);
            });

        public void Delete(RealmObject item) => _realm.Remove(item);

        public void Delete<T>(string primaryKey) where T : RealmObject => _realm.Remove(Query<T>(primaryKey));
        public IQueryable<T> Query<T>() where T : RealmObject => _realm.All<T>();
        public T Query<T>(string primaryKey) where T : RealmObject => _realm.Find<T>(primaryKey);
    }
}