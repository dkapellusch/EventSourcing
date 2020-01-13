using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.KSQL;
using Newtonsoft.Json.Linq;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.VehicleReadService
{
    public class VehicleKsqlTable
    {
        private readonly KsqlQuery _getCountQuery;
        private readonly KsqlQueryExecutor _ksqlQueryExecutor;

        public VehicleKsqlTable(KsqlQueryExecutor ksqlQueryExecutor)
        {
            _ksqlQueryExecutor = ksqlQueryExecutor;
            _getCountQuery = new KsqlQuery
            {
                Ksql = "Select count(counter) from Vehicles_Count group by counter emit changes limit 1;",
                StreamProperties = {KsqlQuery.OffsetEarliest}
            };
        }

        public IAsyncEnumerable<Vehicle> GetVehicleUpdatesAsync(CancellationToken token = default) =>
            _ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
                {
                    Ksql = "Select * from vehicles_all emit changes;"
                },
                ParseVehicle);

        public async Task<List<Vehicle>> GetAllVehiclesAsync()
        {
            var count = await _ksqlQueryExecutor.ExecuteQuery(_getCountQuery, columns => columns.First().Value).EnumerateAsync();
            var allVehicles = await _ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
                    {
                        Ksql = $"Select * from vehicles_all emit changes limit {count[0]};",
                        StreamProperties = {KsqlQuery.OffsetEarliest}
                    },
                    ParseVehicle)
                .EnumerateAsync();


            return allVehicles;
        }

        public async Task<Vehicle> GetVehicleByVinAsync(string vin)
        {
            var results = await _ksqlQueryExecutor.ExecuteQuery(new KsqlQuery
                    {
                        Ksql = $"Select * from vehicles_all where rowkey = '{vin}';"
                    },
                    ParseVehicle)
                .EnumerateAsync();
            return results.FirstOrDefault();
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
            return valueString.RemoveWhitespace().Replace("null", string.Empty).Trim(',');
        }
    }
}