using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EventSourcing.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcing.KSQL
{
    public class KafkaKsqlQueryExecutor
    {
        private readonly KsqlClient _ksqlRestClient;

        public KafkaKsqlQueryExecutor(KsqlClient ksqlRestClient) => _ksqlRestClient = ksqlRestClient;

        public async IAsyncEnumerable<T> ExecuteQuery<T>(KsqlQuery query, Mapper<T> mapper)
        {
            await using var queryStream = await _ksqlRestClient.ExecuteQueryAsync(query);
            using var streamReader = new StreamReader(queryStream);

            var header = GetHeader(streamReader);
            var columns = header.Columns;

            while (!streamReader.EndOfStream)
            {
                var row = GetRow(streamReader);

                if (row.IsNullOrDefault()) yield break;

                yield return mapper(columns.Zip(row.Columns).ToDictionary(kv => kv.First, kv => kv.Second));
            }
        }

        private static Header GetHeader(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadChunk("},");
                var header = line.Substring(1, line.LastIndexOf('}'));
                return JsonConvert.DeserializeObject<HeaderWrapper>(header).Header;
            }

            return null;
        }

        private static Row GetRow(StreamReader streamReader)
        {
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadChunk("},");
                var row = line.Substring(1, line.LastIndexOf('}'));
                var rowWrapper = JsonConvert.DeserializeObject<RowWrapper>(row);
                if (rowWrapper.LimitReached) return null;

                return rowWrapper.Row;
            }

            return null;
        }
    }
}