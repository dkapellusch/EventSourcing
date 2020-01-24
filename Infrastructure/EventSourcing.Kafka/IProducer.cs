using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public interface IProducer<TKey, TPayload> where TPayload : IMessage<TPayload>, new()
    {
        Task<DeliveryResult<TKey, TPayload>> ProduceAsync(TPayload payload, TKey key);

        void Flush(CancellationToken token = default);
    }
}