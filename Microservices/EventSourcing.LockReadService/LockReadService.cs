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

            return currentLock;
        }

        public override async Task ExpiringLocks(Empty request, IServerStreamWriter<Lock> responseStream, ServerCallContext context) =>
            await _lockChangeTracker.GetChanges()
                .Where(l => l.IsInactive())
                .ForEachAsync(async l => await responseStream.WriteAsync(l));
    }
}