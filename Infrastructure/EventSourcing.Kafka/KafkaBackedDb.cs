using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using EventSourcing.Contracts;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public class KafkaBackedDb<TValue> where TValue : class, IMessage<TValue>
    {
        private readonly IDataStore _dataStore;

        public KafkaBackedDb(IDataStore dataStore, KafkaConsumer<TValue> kafkaConsumer)
        {
            _dataStore = dataStore;

            kafkaConsumer.Start();
            kafkaConsumer.Subscription
                .ObserveOn(TaskPoolScheduler.Default)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(m =>
                {
                    var value = m.Value;
                    var currentValue = _dataStore.Get<TValue>(m.Key);

                    if (!(currentValue is null))
                        value.UpdateObject(currentValue);

                    _dataStore.Save(() => value, m.Key);
                    kafkaConsumer.Commit(m.Partition, m.Offset);
                });
        }

        public TValue GetItem(string key) => _dataStore.Get<TValue>(key);

        public IEnumerable<TValue> GetAll() => _dataStore.Query<TValue>();

        public IEnumerable<TValue> GetItems(string key) => _dataStore.Query<TValue>(key);

        public IObservable<TValue> GetChanges() => _dataStore.GetChanges<TValue>();
    }
}