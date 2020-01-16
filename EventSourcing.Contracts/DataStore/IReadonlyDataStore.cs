using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IReadonlyDataStore
    {
        Task<T> Get<T>(string key);
    }

    public interface IReadonlyDataStore<T>
    {
        Task<T> Get(string key);
    }
}