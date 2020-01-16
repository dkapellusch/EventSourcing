using System;

namespace EventSourcing.Contracts.DataStore
{
    public interface IChangeTracking
    {
        IObservable<T> GetChanges<T>();
    }

    public interface IChangeTracking<T>
    {
        IObservable<T> GetChanges();
    }
}