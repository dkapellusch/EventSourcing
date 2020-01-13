using System;

namespace EventSourcing.Contracts.DataStore
{
    public interface IChangeTracking
    {
        IObservable<T> GetChanges<T>() where T : class;
    }

    public interface IChangeTracking<T> where T : class
    {
        IObservable<T> GetChanges();
    }
}