using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using Microsoft.Extensions.Hosting;

namespace EventSourcing.LockWriteService
{
    public class ExpiredLockNotifier : BackgroundService
    {
        private readonly IExpiringDataStore _dataStore;
        private readonly KafkaProducer<string, Lock> _lockProducer;

        public ExpiredLockNotifier(IExpiringDataStore dataStore, KafkaProducer<string, Lock> lockProducer)
        {
            _dataStore = dataStore;
            _lockProducer = lockProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
            await _dataStore.ExpiredKeys
                .Retry()
                .Select(k => k.Split("/"))
                .Where(k => k[0].Equals("locks"))
                .ForEachAsync(async expiredLock =>
                    {
                        var lockKey = expiredLock[1];
                        var lockValue = await _dataStore.Get<Lock>(lockKey);
                        if (!lockValue.IsNotNullOrDefault()) return;

                        await _lockProducer.ProduceAsync(lockValue, lockValue.ResourceId);
                        await _dataStore.Delete<Lock>(lockKey);
                    },
                    stoppingToken);
    }
}