using System;

namespace EventSourcing.RealmDb
{
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
        }
    }
}