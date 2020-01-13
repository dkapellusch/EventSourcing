using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IWriteOnlyDataStore
    {
        Task Set<T>(T value, string key) where T : class;
    }

    public interface IWriteOnlyDataStore<in T> where T : class
    {
        Task Set(T value, string key);
    }
}