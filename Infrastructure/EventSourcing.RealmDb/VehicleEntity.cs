using System.Runtime.Serialization;
using EventSourcing.Contracts;
using Realms;

namespace EventSourcing.RealmDb
{
    public class VehicleEntity : RealmObject, IEntity
    {
        [Ignored] [IgnoreDataMember] public IDataStore DataStore { get; set; }

        public string PrimaryKey
        {
            get => Vin;
            set => Vin = value;
        }

        [PrimaryKey] public string Vin { get; set; }

        public string Model { get; set; }

        public string Make { get; set; }
    }
}