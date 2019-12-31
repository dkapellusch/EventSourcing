using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using EventSourcing.RocksDb.RocksAbstractions;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public class KafkaBackedDb<TValue> where TValue : IMessage<TValue>
    {
        private readonly RockCollection _rockCollection;

        public KafkaBackedDb(RockCollection rockCollection, KafkaConsumer<TValue> kafkaConsumer)
        {
            _rockCollection = rockCollection;

            kafkaConsumer.Start();
            kafkaConsumer.Subscription
                .ObserveOn(TaskPoolScheduler.Default)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(m =>
                {
                    var value = m.Value;
                    var currentValue = _rockCollection.Get<string, TValue>(m.Key);

                    if (!(currentValue is null))
                        value.UpdateObject(currentValue);

                    _rockCollection.Add(m.Key, value);
                    kafkaConsumer.Commit(m.Partition, m.Offset);
                });
        }

        public TValue GetItem(string key) => _rockCollection.Get<string, TValue>(key);

        public IEnumerable<TValue> GetAll() => _rockCollection.GetItems<string, TValue>().Select(pair => pair.value);

        public IEnumerable<TValue> GetItems(string key) => _rockCollection.GetItems<string, TValue>(key, (k, _) => k.Contains(key)).Select(kv => kv.value);

        public IObservable<TValue> GetChanges() => _rockCollection.ChangedDataCaptureStream.OfType<DataChangedEvent<string, TValue>>().Select(dc => dc.Data.value);
    }
}