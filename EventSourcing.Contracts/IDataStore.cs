using System;
using System.Collections.Generic;

namespace EventSourcing.Contracts
{
    public interface IDataStore
    {
        void Save<T>(Func<T> transaction) where T : class, IEntity;

        T Get<T>(string primaryKey) where T : class, IEntity;

        void Delete<T>(string primaryKey) where T : class, IEntity;

        IEnumerable<T> Query<T>() where T : class, IEntity;

        IEnumerable<T> Query<T>(string startingKey) where T : class, IEntity;
    }
}