using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IReadonlyDataStore
    {
        Task<T> Get<T>(string key) where T : class;

        Task<IEnumerable<T>> Query<T>() where T : class;

        Task<IEnumerable<T>> Query<T>(string startingKey) where T : class;
    }

    public interface IReadonlyDataStore<T> where T : class
    {
        Task<T> Get(string key);

        Task<IEnumerable<T>> Query();

        Task<IEnumerable<T>> Query(string startingKey);
    }
}