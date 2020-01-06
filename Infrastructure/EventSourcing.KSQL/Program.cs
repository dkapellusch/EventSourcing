using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourcing.KSQL
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var queryExecutor = new KafkaKsqlQueryExecutor(new KsqlClient(new HttpClient {BaseAddress = new Uri("http://localhost:8088/query")}));
            var vehicleTable = new VehicleKsqlTable(queryExecutor);
            var vs = await vehicleTable.GetAllVehicles();
            var v = await vehicleTable.GetVehicleByVin("31145678912345678");
            Console.WriteLine(vs[1].Make);
        }
    }
}