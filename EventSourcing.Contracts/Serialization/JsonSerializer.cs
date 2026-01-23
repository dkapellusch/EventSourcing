using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace EventSourcing.Contracts.Serialization
{
    public sealed class JsonSerializer<T> : ISerializer<T>
    {
        private readonly JsonSerializerOptions _settings;

        public JsonSerializer() => _settings = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public T Deserialize(byte[] serializedData) => System.Text.Json.JsonSerializer.Deserialize<T>(serializedData, _settings);

        public byte[] Serialize(T dataToSerialize) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dataToSerialize, _settings);
    }

    public sealed class NewtonJsonSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] serializedData)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(serializedData));
            }
            catch
            {
                return default;
            }
        }

        public byte[] Serialize<T>(T dataToSerialize)
        {
            try
            {
                return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dataToSerialize));
            }
            catch
            {
                return default;
            }
        }
    }

    public sealed class JsonSerializer : ISerializer
    {
        private readonly JsonSerializerOptions _settings;

        public JsonSerializer() => _settings = new JsonSerializerOptions
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public T Deserialize<T>(byte[] serializedData) => serializedData is null ? default : System.Text.Json.JsonSerializer.Deserialize<T>(serializedData, _settings);

        public byte[] Serialize<T>(T dataToSerialize) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(dataToSerialize, _settings);
    }
}