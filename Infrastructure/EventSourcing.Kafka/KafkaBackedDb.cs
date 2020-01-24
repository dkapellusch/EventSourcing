using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts.Extensions;
using EventSourcing.RocksDb.RocksAbstractions;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public class KafkaBackedDb<TValue> where TValue : IMessage<TValue>, new()
    {
        private RocksStore _dataStore;

        public KafkaBackedDb(RocksStore dataStore, Consumer<TValue> consumer)
        {
            _dataStore = dataStore;
            _dataStore.GetChanges<Offset>().Subscribe(offset => Console.WriteLine(offset));

            var offsetKey = $"{typeof(TValue).Name}.offset";
            var currentOffset = _dataStore.Get<long>(offsetKey).Result;

            consumer.SeekToOffset(currentOffset);
            consumer.Start();
            consumer.Subscription
                .ObserveOn(TaskPoolScheduler.Default)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(async message =>
                {
                    var value = message.Value;
                    var currentValue = await _dataStore.Get<TValue>(message.Key);

                    if (!(currentValue is null))
                        value.UpdateObject(currentValue);

                    await _dataStore.Set(value, message.Key);
                    await _dataStore.Set(message.Offset.Value, offsetKey);
                });
        }

        public IObservable<TValue> GetChanges() => _dataStore.GetChanges<TValue>();

        public Task<TValue> Get(string key) => _dataStore.Get<TValue>(key);
    }
}