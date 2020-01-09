using System;
using Confluent.Kafka;
using EventSourcing.Contracts.Serialization;

namespace EventSourcing.Kafka
{
    internal sealed class KafkaSerializer<TMessage> : Confluent.Kafka.ISerializer<TMessage>, IDeserializer<TMessage>
    {
        private readonly IMessageSerializer<TMessage> _messageSerializer;

        public KafkaSerializer(IMessageSerializer<TMessage> messageSerializer) => _messageSerializer = messageSerializer;

        public TMessage Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            try
            {
                return isNull || data.IsEmpty || data.Length <= 0
                    ? default
                    : _messageSerializer.Deserialize(data.ToArray());
            }
            catch
            {
                return default;
            }
        }

        public byte[] Serialize(TMessage data, SerializationContext context)
        {
            try
            {
                return data is null
                    ? null
                    : _messageSerializer.Serialize(data);
            }
            catch
            {
                return null;
            }
        }
    }
}