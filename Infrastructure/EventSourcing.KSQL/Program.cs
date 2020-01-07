using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace EventSourcing.KSQL
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
        //     var queryExecutor = new KafkaKsqlQueryExecutor(new KsqlClient(new HttpClient {BaseAddress = new Uri("http://localhost:8088/query")}));
        //     var vs = await vehicleTable.GetAllVehiclesAsync();
        //     var v = await vehicleTable.GetVehicleByVinAsync("31145678912345678");
        //     Console.WriteLine(vs[1].Make);
        }
    }
}