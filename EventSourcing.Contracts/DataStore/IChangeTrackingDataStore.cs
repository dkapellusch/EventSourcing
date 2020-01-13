namespace EventSourcing.Contracts.DataStore
{
    public interface IChangeTrackingDataStore : IDataStore, IChangeTracking
    {
    }
}