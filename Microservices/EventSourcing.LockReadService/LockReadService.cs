using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using Grpc.Core;
using Empty = EventSourcing.Contracts.Empty;

namespace EventSourcing.LockReadService
{
    public class LockReadService : LockRead.LockReadBase
    {
        private readonly IExpiringDataStore _dataStore;

        public LockReadService(IExpiringDataStore dataStore) => _dataStore = dataStore;

        public override async Task<Lock> GetLock(LockRequest request, ServerCallContext context)
        {
            var currentLock = await _dataStore.Get<Lock>(request.ResourceId);

            if (currentLock.IsNullOrDefault() || currentLock.Expiry.ToDateTime() < DateTime.UtcNow || currentLock.Released)
                currentLock = new Lock();

            return currentLock;
        }

        public override async Task ExpiringLocks(Empty request, IServerStreamWriter<Lock> responseStream, ServerCallContext context)
        {
            await _dataStore.ExpiredKeys
                .Select(k => k.Split("/"))
                .Where(k => k[0].Equals("locks"))
                .ForEachAsync(async expiredLock =>
                    {
                        var lockKey = expiredLock[1];
                        var lockValue = await _dataStore.Get<Lock>(lockKey);
                        await responseStream.WriteAsync(lockValue);
                        await _dataStore.Delete<Lock>(lockKey);
                    },
                    context.CancellationToken);
        }
    }
}