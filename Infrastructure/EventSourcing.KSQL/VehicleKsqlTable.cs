using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using Newtonsoft.Json.Linq;

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
                Ksql = "Select count(counter) from Vehicles_Count group by counter emit changes limit 1;",
                StreamProperties = {{"ksql.streams.auto.offset.reset", "earliest"}}
            };
        }

        public async Task<List<Vehicle>> GetAllVehicles()
        {
            var count = await Enumerate(_ksqlQueryExecutor.ExecuteQuery(_getCountQuery, columns => columns.First().Value));
            var allVehicles = await Enumerate(_ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
                {
                    Ksql = $"Select * from vehicles_all emit changes limit {count[0]};",
                    StreamProperties = {KsqlQuery.OffsetEarliest}
                },
                ParseVehicle));

            return allVehicles;
        }

        public async Task<Vehicle> GetVehicleByVin(string vin)
        {
            var results = await Enumerate(_ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
                {
                    Ksql = $"Select * from vehicles_all where rowkey = '{vin}';"
                },
                ParseVehicle));
            return results.FirstOrDefault();
        }

        private static async Task<List<T>> Enumerate<T>(IAsyncEnumerable<T> asyncEnumerable)
        {
            var items = new List<T>();

            await foreach (var element in asyncEnumerable) items.Add(element);

            return items;
        }

        private static Vehicle ParseVehicle(IDictionary<string, dynamic> columns) =>
            new Vehicle
            {
                Vin = GetColumnValue(columns["VIN"]),
                Model = GetColumnValue(columns["MODEL"]),
                Make = GetColumnValue(columns["MAKE"]),
                LocationCode = GetColumnValue(columns["LOCATIONCODE"])
            };

        private static string GetColumnValue(dynamic value)
        {
            Type valueType = value.GetType();
            if (valueType == typeof(JArray)) value = value.Last;

            string valueString = value?.ToString() ?? string.Empty;
            return valueString.RemoveWhitespace().Replace("null,", string.Empty);
        }
    }
}