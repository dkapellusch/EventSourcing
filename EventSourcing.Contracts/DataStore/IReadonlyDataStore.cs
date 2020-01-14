using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IReadonlyDataStore
    {
        Task<T> Get<T>(string key) where T : class;
    }

    public interface IReadonlyDataStore<T> where T : class
    {
        Task<T> Get(string key);
    }
}