namespace EventSourcing.Contracts.DataStore
{
    public interface IChangeTrackingDataStore : IReadonlyDataStore, IWriteOnlyDataStore, IChangeTracking
    {
    }

    public interface IChangeTrackingDataStore<T> : IReadonlyDataStore<T>, IWriteOnlyDataStore<T>, IChangeTracking<T>
    {
    }
}