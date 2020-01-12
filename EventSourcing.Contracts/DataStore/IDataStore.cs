using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IDataStore
    {
        Task Set<T>(T value, string key) where T : class;

        Task<T> Get<T>(string key) where T : class;

        Task Delete<T>(string key) where T : class;

        Task<IEnumerable<T>> Query<T>() where T : class;

        Task<IEnumerable<T>> Query<T>(string startingKey) where T : class;
    }
}