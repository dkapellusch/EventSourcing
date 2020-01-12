using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Microsoft.Extensions.Hosting;

namespace EventSourcing.LockWriteService
{
    public class ExpiredLockNotifier : IHostedService
    {
        private readonly LockRead.LockReadClient _lockReadClient;
        private readonly KafkaProducer<string, Lock> _lockProducer;
        private Thread _monitorThread;

        public ExpiredLockNotifier(LockRead.LockReadClient lockReadClient, KafkaProducer<string, Lock> lockProducer)
        {
            _lockReadClient = lockReadClient;
            _lockProducer = lockProducer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _monitorThread = new Thread(MonitorExpiringLocks);
            _monitorThread.Start();
            return Task.CompletedTask;
        }

        private async void MonitorExpiringLocks()
        {
            await _lockReadClient.ExpiringLocks(new Empty())
                .ResponseStream
                .AsObservable()
                .Retry()
                .ForEachAsync(
                    async expiredLock => await _lockProducer.ProduceAsync(expiredLock, expiredLock.ResourceId)
                );
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _monitorThread.Abort();
            return Task.CompletedTask;
        }
    }
}