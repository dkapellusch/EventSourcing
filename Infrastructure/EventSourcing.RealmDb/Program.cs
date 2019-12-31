using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using EventSourcing.RocksDb.RocksAbstractions;
using EventSourcing.RocksDb.Serialization;
using Realms;

namespace EventSourcing.RealmDb
{
    public interface IDataStore
    {
        public Task Save<T>(T entity) where T : Entity;
        public Task Get<T>(string primaryKey) where T : Entity;
        public Task Delete<T>(string primaryKey) where T : Entity;
        public Task<IEnumerable<T>> Query<T>() where T : Entity;
    }

    public abstract class Entity : INotifyPropertyChanged
    {
        protected Entity()
        {
            PropertyChanged += (sender, args) =>
            {
                Updated = true;
                if (Attached) Save();
            };
        }

        public void Save()
        {
            if (!Attached) throw new InvalidOperationException("Entity has not yet been attached, please call Attach first.");
            if (Updated) DataStore.Save(this);
        }

        private IDataStore DataStore { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract string PrimaryKey { get; }
        public bool Attached { get; private set; }
        internal bool Updated { get; private set; }

        public void Attach(IDataStore dataStore)
        {
            Attached = true;
            DataStore = dataStore;
        }
    }

    public class VehicleObject : RealmObject
    {
        [PrimaryKey] public string Vin { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        [Indexed] public string LocationCode { get; set; }
    }

    public class VehicleEntity : RocksEntity
    {
        public VehicleEntity(string vin, string model, string make)
        {
            Vin = vin;
            Model = model;
            Make = make;
        }

        public override string PrimaryKey => Vin;

        public override bool AutoSave => true;

        public string Vin { get; }
        public string Model { get; set; }
        public string Make { get; set; }
    }

    public class Program
    {
        public static void Main()
        {
            var rock = new Rocky(new RocksDatabase("./rockOrm.db"), new NewtonJsonSerializer());
            RocksEntity.ConnectDatabase(rock);

            var v = new VehicleEntity("123456", "Camry", "Toyota");

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