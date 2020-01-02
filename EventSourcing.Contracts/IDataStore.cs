using System.Collections.Generic;

namespace EventSourcing.Contracts
{
    public interface IDataStore
    {
        public void Save<T>(T entity) where T : class, IEntity;
        public T Get<T>(string primaryKey) where T :  class, IEntity;
        public void Delete<T>(string primaryKey) where T :  class, IEntity;
        public IEnumerable<T> Query<T>() where T :  class, IEntity;
    }
}