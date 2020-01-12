using System;

namespace EventSourcing.Contracts.DataStore
{
    public interface IChangeTrackingDataStore : IDataStore
    {
        IObservable<T> GetChanges<T>() where T : class;
    }
}