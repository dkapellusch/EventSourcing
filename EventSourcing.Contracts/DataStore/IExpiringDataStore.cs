using System;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface IExpiringDataStore : IDataStore
    {
        Task Set<T>(T value, string key, TimeSpan timeToLive) where T : class;

        IObservable<string> ExpiredKeys { get; }
    }
}