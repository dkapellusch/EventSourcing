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
        public VehicleEntity(string vin, string model, string make)
        {
            Vin = vin;
            Model = model;
            Make = make;
        }

        public IDataStore DataStore { get; set; }

        public string PrimaryKey
        {
            get => Vin;
            set => Vin = value;
        }

        public bool Attached { get; set; }

        public string Vin { get; private set; }
        public string Model { get; set; }
        public string Make { get; set; }
    }

    public class Program
    {
        public static void Main()
        {
            var rock = new RocksStore(new RocksDatabase("./rockOrm.db"), new NewtonJsonSerializer());
            var v = new VehicleEntity("123456", "Camry", "Toyota");

            rock.Save(v);
            Console.WriteLine(rock.Get<VehicleEntity>("123456").Make);
            v.Make = "Honda";
            Console.WriteLine(rock.Get<VehicleEntity>("123456").Make);
            // var store = new RealmStore();
            // var v = store
            //     .Query<VehicleObject>()
            //     .Where(vehicle => vehicle.Vin.Contains("1"))
            //     .ToList();
            // Console.WriteLine(v.First().Model);
        }
    }
}