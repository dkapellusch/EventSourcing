using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts.Extensions;
using Newtonsoft.Json;

namespace EventSourcing.KSQL
{
    public class KsqlQueryExecutor
    {
        private readonly KsqlClient _ksqlRestClient;

        public KsqlQueryExecutor(KsqlClient ksqlRestClient) => _ksqlRestClient = ksqlRestClient;

        public async IAsyncEnumerable<T> ExecuteQuery<T>(KsqlQuery query, Mapper<T> mapper, [EnumeratorCancellation] CancellationToken token = default, bool isQuery = true)
        {
            await using var queryStream = await _ksqlRestClient.ExecuteQueryAsync(query, token, isQuery);
            using var streamReader = new StreamReader(queryStream);

            var header = ParseKeyToObject<Header>(streamReader, "header");

            if (header.IsNullOrDefault()) yield break;

            var columns = header.Columns;

            while (!streamReader.EndOfStream)
            {
                var row = ParseKeyToObject<Row>(streamReader, "row");

                if (row.IsNullOrDefault()) yield break;

                yield return mapper(columns.Zip(row.Columns).ToDictionary(kv => kv.First, kv => kv.Second));
            }
        }

        public async Task<string> ExecuteQuery(KsqlQuery query, [EnumeratorCancellation] CancellationToken token = default, bool isQuery = true)
        {
            await using var queryStream = await _ksqlRestClient.ExecuteQueryAsync(query, token, isQuery);
            using var streamReader = new StreamReader(queryStream);
            return await streamReader.ReadToEndAsync();
        }

        private static T ParseKeyToObject<T>(StreamReader streamReader, string key) where T : class
        {
            streamReader.SeekTo($"\"{key}\":");
            var chunk = streamReader.ReadChunk("},");
            var lastIndex = chunk.LastIndexOf('}');
            if (lastIndex < 0) return null;

            var jsonObject = chunk.Substring(0, lastIndex);
            return JsonConvert.DeserializeObject<T>(jsonObject);
        }
    }
}