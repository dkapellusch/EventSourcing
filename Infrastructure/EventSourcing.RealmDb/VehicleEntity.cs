using Realms;

namespace EventSourcing.RealmDb
{
    public class VehicleEntity : RealmObject
    {
        [PrimaryKey] public string Vin { get; set; }

        public string Model { get; set; }

        public string Make { get; set; }
    }
}