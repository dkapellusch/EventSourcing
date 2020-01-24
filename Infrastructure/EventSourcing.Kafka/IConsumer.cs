using System;
using System.Threading;
using Confluent.Kafka;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public interface IConsumer<TPayload> where TPayload : IMessage<TPayload>, new()
    {
        IObservable<ConsumeResult<string, TPayload>> Subscription { get; }

        void Start(CancellationToken token = default);

        void SeekToOffset(long offset);

        bool Commit(int partition, long offset);
    }
}