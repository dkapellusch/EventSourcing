using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IDataStore : IReadonlyDataStore, IWriteOnlyDataStore, IQueryableDataStore
    {
        Task Delete<T>(string key);
    }

    public interface IDataStore<T> : IReadonlyDataStore<T>, IWriteOnlyDataStore<T>, IQueryableDataStore<T>
    {
        Task Delete(string key);
    }
}