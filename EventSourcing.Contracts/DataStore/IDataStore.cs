using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IDataStore : IReadonlyDataStore, IWriteOnlyDataStore, IQueryableDataStore
    {
        Task Delete<T>(string key) where T : class;
    }

    public interface IDataStore<T> : IReadonlyDataStore<T>, IWriteOnlyDataStore<T>, IQueryableDataStore<T> where T : class
    {
        Task Delete(string key);
    }
}