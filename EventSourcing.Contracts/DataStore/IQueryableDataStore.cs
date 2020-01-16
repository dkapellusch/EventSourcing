using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IQueryableDataStore
    {
        Task<IEnumerable<T>> Query<T>();

        Task<IEnumerable<T>> Query<T>(string startingKey);
    }

    public interface IQueryableDataStore<T>
    {
        Task<IEnumerable<T>> Query();

        Task<IEnumerable<T>> Query(string startingKey);
    }
}