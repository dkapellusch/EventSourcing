using System;
using System.Collections.Generic;

namespace EventSourcing.Contracts
{
    public interface IDataStore
    {
        void Save<T>(Func<T> transaction, string primaryKey) where T : class;

        T Get<T>(string primaryKey) where T : class;

        void Delete<T>(string primaryKey) where T : class;

        IEnumerable<T> Query<T>() where T : class;

        IEnumerable<T> Query<T>(string startingKey) where T : class;

        IObservable<T> GetChanges<T>() where T : class;
    }
}