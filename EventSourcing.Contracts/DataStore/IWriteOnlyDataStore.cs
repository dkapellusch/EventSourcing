using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IWriteOnlyDataStore
    {
        Task Set<T>(T value, string key);
    }

    public interface IWriteOnlyDataStore<in T>
    {
        Task Set(T value, string key);
    }
}