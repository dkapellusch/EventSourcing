using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using Google.Protobuf;

namespace EventSourcing.Kafka
{
    public class KafkaBackedDb<TValue> where TValue : class, IMessage<TValue>
    {
        private readonly IChangeTrackingDataStore _dataStore;

        public KafkaBackedDb(IChangeTrackingDataStore dataStore, KafkaConsumer<TValue> kafkaConsumer)
        {
            _dataStore = dataStore;

            kafkaConsumer.Start();
            kafkaConsumer.Subscription
                .ObserveOn(TaskPoolScheduler.Default)
                .SubscribeOn(TaskPoolScheduler.Default)
                .Subscribe(m =>
                {
                    var value = m.Value;
                    var currentValue = _dataStore.Get<TValue>(m.Key).Result;

                    if (!(currentValue is null))
                        value.UpdateObject(currentValue);

                    _dataStore.Set(value, m.Key);
                    kafkaConsumer.Commit(m.Partition, m.Offset);
                });
        }

        public TValue GetItem(string key) => _dataStore.Get<TValue>(key).Result;

        public IEnumerable<TValue> GetAll() => _dataStore.Query<TValue>().Result;

        public IEnumerable<TValue> GetItems(string key) => _dataStore.Query<TValue>(key).Result;

        public IObservable<TValue> GetChanges() => _dataStore.GetChanges<TValue>();
    }
}