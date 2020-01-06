using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using EventSourcing.Contracts;

namespace EventSourcing.KSQL
{
    public class VehicleKsqlTable
    {
        private readonly KafkaKsqlQueryExecutor _ksqlQueryExecutor;
        private readonly KsqlQuery _getCountQuery;

        public VehicleKsqlTable(KafkaKsqlQueryExecutor ksqlQueryExecutor)
        {
            _ksqlQueryExecutor = ksqlQueryExecutor;
            _getCountQuery = new KsqlQuery
            {
                Ksql = "Select count(count) from Vehicle_Count group by count emit changes limit 1;",
                StreamProperties = {{"ksql.streams.auto.offset.reset", "earliest"}}
            };
        }

        public async Task GetAllVehicles()
        {
            var count = await Enumerate(_ksqlQueryExecutor.ExecuteQuery(_getCountQuery, columns => columns.First().Value));
            var allVehicles = await Enumerate(_ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
            {
                Ksql = $"Select * from vehicles_all emit changes limit {count[0]};",
                StreamProperties = {{"ksql.streams.auto.offset.reset", "earliest"}}
            }, columns => new Vehicle {Vin = columns["VIN"], Model = columns["MODEL"][0] ?? "", LocationCode = columns["LOCATIONCODE"][0] ?? ""}));

            Console.WriteLine(allVehicles[0]);
        }

        private string RemoveBrackets(string value) => value.Trim('[', ']');

        private async Task<List<T>> Enumerate<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();

            await foreach (var element in asyncEnumerable)
            {
                items.Add(element);
            }

            return items;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            var queryExecutor = new KafkaKsqlQueryExecutor(new KsqlClient(new HttpClient {BaseAddress = new Uri("http://localhost:8088/query")}));
            var vehicleThing = new VehicleKsqlTable(queryExecutor);
            await vehicleThing.GetAllVehicles();
        }
    }
}