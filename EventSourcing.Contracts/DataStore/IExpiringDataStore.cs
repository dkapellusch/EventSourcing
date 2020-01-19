using System;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IExpiringDataStore : IDataStore
    {
        IObservable<string> ExpiredKeys { get; }

        Task Set<T>(T value, string key, TimeSpan timeToLive);
    }
}