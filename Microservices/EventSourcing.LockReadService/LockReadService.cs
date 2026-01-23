using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.LockReadService
{
    public class LockReadService : LockRead.LockReadBase
    {
        private readonly KafkaBackedDb<Lock> _db;

        public LockReadService(KafkaBackedDb<Lock> db) => _db = db;

        public override async Task<Lock> GetLock(LockRequest request, ServerCallContext context)
        {
            var currentLock = await _db.Get(request.ResourceId);

            if (currentLock.IsNullOrDefault() || currentLock.IsInactive())
                currentLock = new Lock();

            currentLock.LockId = string.Empty;
            return currentLock;
        }

        public override Task ExpiringLocks(Empty request, IServerStreamWriter<Lock> responseStream, ServerCallContext context) =>
            _db.GetChanges()
                .ObserveOn(TaskPoolScheduler.Default)
                .Where(l => l.IsInactive())
                .ForEachAsync(async l =>
                    {
                        try
                        {
                            await responseStream.WriteAsync(l);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    },
                    context.CancellationToken);
    }
}