using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IDataStore : IReadonlyDataStore, IWriteOnlyDataStore
    {
        Task Delete<T>(string key) where T : class;
    }

    public interface IDataStore<T> : IReadonlyDataStore<T>, IWriteOnlyDataStore<T> where T : class
    {
        Task Delete(string key);
    }
}