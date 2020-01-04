using System.Collections.Generic;
using Newtonsoft.Json;

namespace EventSourcing.KSQL
{
    public delegate TRow Mapper<TRow>(string[] columns);

    public sealed class Row
    {
        public object[] Columns { get; set; }
    }

    public sealed class StreamResponse
    {
        public Row Row { get; set; }
    }

    public sealed class KsqlQuery
    {
        [JsonProperty("ksql")] public string Ksql { get; set; }

        [JsonProperty("streamsProperties")] public Dictionary<string, string> StreamProperties { get; } = new Dictionary<string, string>();
    }
}