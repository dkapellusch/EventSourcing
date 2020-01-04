using System;
using System.Runtime.Serialization;
using EventSourcing.Contracts;
using EventSourcing.RocksDb.RocksAbstractions;
using EventSourcing.RocksDb.Serialization;
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

    public class Program
    {
        public static void Main()
        {
            // var realm = new RocksStore(new RocksDatabase("./rockOrm.db"), new NewtonJsonSerializer());
            var realm = new RealmStore();
            var v = realm.Get<VehicleEntity>("123456") ?? new VehicleEntity {Vin = "123456", DataStore = realm};

            v.DataStore.Save(() =>
            {
                v.Make = "Honda";
                return v;
            });
            Console.WriteLine(realm.Get<VehicleEntity>("123456").Make);
            // var store = new RealmStore();
            // var v = store
            //     .Query<VehicleObject>()
            //     .Where(vehicle => vehicle.Vin.Contains("1"))
            //     .ToList();
            // Console.WriteLine(v.First().Model);
        }
    }
}