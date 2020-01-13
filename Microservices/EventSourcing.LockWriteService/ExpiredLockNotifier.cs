using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using Microsoft.Extensions.Hosting;

namespace EventSourcing.LockWriteService
{
    public class ExpiredLockNotifier : BackgroundService
    {
        private readonly LockRead.LockReadClient _lockReadClient;
        private readonly KafkaProducer<string, Lock> _lockProducer;

        public ExpiredLockNotifier(LockRead.LockReadClient lockReadClient, KafkaProducer<string, Lock> lockProducer)
        {
            _lockReadClient = lockReadClient;
            _lockProducer = lockProducer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _lockReadClient.ExpiringLocks(new Empty())
                .ResponseStream
                .AsObservable()
                .Retry()
                .ForEachAsync(expiredLock => _lockProducer.ProduceAsync(expiredLock, expiredLock.ResourceId), stoppingToken);
        }
    }
}