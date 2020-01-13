using System;
using System.Threading.Tasks;

namespace EventSourcing.Contracts.DataStore
{
    public interface ITransactionalDataStore : IDataStore
    {
        Task Save<T>(Func<T> transaction, string primaryKey) where T : class;
    }

    public interface ITransactionalDataStore<T> : IDataStore<T> where T : class
    {
        Task Save(Func<T> transaction, string primaryKey);
    }
}