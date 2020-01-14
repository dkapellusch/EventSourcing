using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using Grpc.Core;
using Empty = EventSourcing.Contracts.Empty;

namespace EventSourcing.LockReadService
{
    public class LockReadService : LockRead.LockReadBase
    {
        private readonly IReadonlyDataStore<Lock> _lockStore;
        private readonly IChangeTracking<Lock> _lockChangeTracker;

        public LockReadService(IReadonlyDataStore<Lock> lockStore, IChangeTracking<Lock> lockChangeTracker)
        {
            _lockStore = lockStore;
            _lockChangeTracker = lockChangeTracker;
        }

        public override async Task<Lock> GetLock(LockRequest request, ServerCallContext context)
        {
            var currentLock = await _lockStore.Get(request.ResourceId);

            if (currentLock.IsNullOrDefault() || currentLock.IsInactive())
                currentLock = new Lock();

            // Do not expose LockId so only the owner can release it.
            currentLock.LockId = string.Empty;
            return currentLock;
        }

        public override async Task ExpiringLocks(Empty request, IServerStreamWriter<Lock> responseStream, ServerCallContext context) =>
            await _lockChangeTracker.GetChanges()
                .Where(l =>
                {
                    if (l.IsInactive()) return true;

                    Console.WriteLine($"Skipping {l}, not expired.");
                    return false;
                })
                .TakeWhile(_ => !context.CancellationToken.IsCancellationRequested)
                .ForEachAsync(async l =>
                {
                    try
                    {
                        await responseStream.WriteAsync(l);
                        Console.WriteLine($"Expiring {l}.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                });
    }
}