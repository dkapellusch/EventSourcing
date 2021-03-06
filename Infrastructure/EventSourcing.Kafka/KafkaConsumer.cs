using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Confluent.Kafka;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Contracts.Serialization;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public sealed class KafkaConsumer<TPayload> : IDisposable where TPayload : IMessage<TPayload>, new()
    {
        private readonly IConsumer<string, TPayload> _consumer;
        private readonly string _topicName;

        public KafkaConsumer(ConsumerConfig config, IMessageSerializer<TPayload> serializer, string topicName = null)
        {
            _topicName = topicName ?? $"{typeof(TPayload).Name}s";
            _consumer = new ConsumerBuilder<string, TPayload>(config)
                .SetValueDeserializer(new KafkaSerializer<TPayload>(serializer))
                .Build();
        }

        public IObservable<ConsumeResult<string, TPayload>> Subscription { get; private set; }

        public void Dispose()
        {
            _consumer?.Dispose();
        }

        public void Start(CancellationToken token = default)
        {
            if (!_consumer.Subscription.Contains(_topicName)) _consumer.Subscribe(_topicName);

            if (Subscription != null) return;

            Subscription =
                Observable.Start(ReadOne)
                    .Expand(lv => Observable.Start(ReadOne))
                    .Where(m => m.IsNotNullOrDefault())
                    .TakeWhile(r => !token.IsCancellationRequested);
        }

        private ConsumeResult<string, TPayload> ReadOne()
        {
            try
            {
                var consumeResult = _consumer.Consume(TimeSpan.FromMilliseconds(5000));

                if (consumeResult?.Message is null || consumeResult.IsPartitionEOF || consumeResult.Value is null) return null;

                return consumeResult;
            }
            catch (Exception)
            {
            }

            return default;
        }

        public void SeekToOffset(long offset)
        {
            var partitions = _consumer.Assignment.Select(a => a.Partition).ToArray();

            if (!partitions.Any()) partitions = new[] {new Partition()};

            foreach (var partition in partitions) Seek(partition, offset);
        }

        public IEnumerable<TopicPartitionOffset> GetOffsetsFromTime(DateTime time)
        {
            var partitions = _consumer.Assignment.Select(a => a.Partition);
            return partitions.SelectMany(p => _consumer.OffsetsForTimes(new[] {new TopicPartitionTimestamp(_topicName, p, new Timestamp(time))}, TimeSpan.FromSeconds(30)));
        }

        public bool Commit(int partition, long offset)
        {
            try
            {
                _consumer.Commit(new[] {new TopicPartitionOffset(_topicName, partition, offset + 1)});

                return true;
            }
            catch
            {
                return false;
            }
        }

        private void Seek(Partition partition, Offset offset)
        {
            try
            {
                var topicOffset = new TopicPartitionOffset(_topicName, partition, offset);

                _consumer.Assign(topicOffset);
                _consumer.Seek(topicOffset);
            }
            catch (KafkaException)
            {
            }
        }
    }
}