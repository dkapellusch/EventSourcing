using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.KSQL;
using Grpc.Core;

namespace EventSourcing.LockReadService
{
    public class LockReadService : LockRead.LockReadBase
    {
        private readonly KsqlStore<Lock> _kafkaDb;

        public LockReadService(KsqlStore<Lock> kafkaDb) => _kafkaDb = kafkaDb;

        public override async Task<Lock> GetLock(LockRequest request, ServerCallContext context)
        {
            var currentLock = await _kafkaDb.Get(request.ResourceId);

            if (currentLock.IsNullOrDefault() || currentLock.IsInactive())
                currentLock = new Lock();

            // Do not expose LockId so only the owner can release it.
            currentLock.LockId = string.Empty;
            return currentLock;
        }

        public override Task ExpiringLocks(Empty request, IServerStreamWriter<Lock> responseStream, ServerCallContext context) =>
            _kafkaDb.GetChanges()
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