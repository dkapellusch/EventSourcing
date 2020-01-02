using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace EventSourcing.RocksDb.Serialization
{
    public sealed class JsonSerializer<T> : ISerializer<T>
    {
        private readonly JsonSerializerOptions _settings;

        public JsonSerializer() => _settings = new JsonSerializerOptions {ReadCommentHandling = JsonCommentHandling.Allow, IgnoreNullValues = true, PropertyNameCaseInsensitive = true};

        public T Deserialize(byte[] serializedData) => System.Text.Json.JsonSerializer.Deserialize<T>(serializedData, _settings);

        public byte[] Serialize(T dataToSerialize) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dataToSerialize, _settings);
    }

    public sealed class NewtonJsonSerializer : ISerializer
    {

        public T Deserialize<T>(byte[] serializedData) => JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(serializedData));

        public byte[] Serialize<T>(T dataToSerialize) => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSerialize));
    }

    public sealed class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _settings;

        public JsonSerializer() => _settings = new JsonSerializerOptions {IgnoreNullValues = true, PropertyNameCaseInsensitive = true};

        public T Deserialize<T>(byte[] serializedData) => serializedData is null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(serializedData, _settings);

        public byte[] Serialize<T>(T dataToSerialize) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dataToSerialize, _settings);
    }
}