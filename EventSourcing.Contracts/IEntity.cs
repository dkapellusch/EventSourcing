using System.Runtime.Serialization;

namespace EventSourcing.Contracts
{
    public interface IEntity
    {
        [IgnoreDataMember] IDataStore DataStore { get; set; }

        string PrimaryKey { get; set; }
    }
}