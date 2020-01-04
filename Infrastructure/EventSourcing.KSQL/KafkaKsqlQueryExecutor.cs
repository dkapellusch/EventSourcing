using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EventSourcing.KSQL
{
    public class KafkaKsqlQueryExecutor
    {
        private readonly KsqlClient _ksqlRestClient;

        public KafkaKsqlQueryExecutor(KsqlClient ksqlRestClient)
        {
            _ksqlRestClient = ksqlRestClient;
        }

        public async IAsyncEnumerable<T> ExecuteQuery<T>(KsqlQuery query, Mapper<T> mapper)
        {
            await using var queryStream = await _ksqlRestClient.ExecuteQueryAsync(query);
            using var streamReader = new StreamReader(queryStream);

            while (!streamReader.EndOfStream)
            {
                var line = await streamReader.ReadLineAsync();

                if (string.IsNullOrEmpty(line))
                    continue;

                var streamResponse = JsonConvert.DeserializeObject<StreamResponse>(line);

                if (streamResponse.Row?.Columns is null || !streamResponse.Row.Columns.Any())
                    break;

                var values = streamResponse.Row.Columns.Skip(2).SelectMany(FlattenNestedValues).ToArray();
                var payload = mapper(values);
                yield return payload;
            }
        }

        private static List<string> FlattenNestedValues(object someObj)
        {
            if (!(someObj is JObject jObject))
                return new List<string> {someObj?.ToString() ?? string.Empty};

            var values = new List<string>();

            foreach (var property in jObject.Properties())
            {
                if (property.Value is JObject complexObject)
                    values.AddRange(FlattenNestedValues(complexObject));

                else if (property.HasValues)
                    values.Add(property.Value.Value<string>());
            }

            return values;
        }
    }
}