using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace EventSourcing.KSQL
{
    public delegate TRow Mapper<TRow>(Dictionary<string, dynamic> columns);

    public sealed class Header
    {
        public string QueryId { get; set; }

        public string Schema { get; set; }

        public string[] Columns => Schema.Split(", ").Select(column => column.Split(" ")[0].Trim('`')).ToArray();
    }

    public sealed class Row
    {
        public object[] Columns { get; set; }
    }

    public sealed class KsqlQuery
    {
        public static KeyValuePair<string, string> OffsetEarliest => new KeyValuePair<string, string>("ksql.streams.auto.offset.reset", "earliest");

        [JsonProperty("ksql")] public string Ksql { get; set; }

        [JsonProperty("streamsProperties")] public Dictionary<string, string> StreamProperties { get; set; } = new Dictionary<string, string>();
    }
}