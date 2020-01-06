using System;

namespace EventSourcing.RealmDb
{
    public class Program
    {
        public static void Main()
        {
            // var realm = new RocksStore(new RocksDatabase("./rockOrm.db"), new NewtonJsonSerializer());
            var realm = new RealmStore();
            var v = realm.Get<VehicleEntity>("123456") ?? new VehicleEntity {Vin = "123456"};

            realm.Save(() =>
            {
                v.Make = "Honda";
                return v;
            }, v.Vin);
            Console.WriteLine(realm.Get<VehicleEntity>("123456").Make);
        }
    }
}