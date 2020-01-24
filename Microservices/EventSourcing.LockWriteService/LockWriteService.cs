using System;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using EventSourcing.Redis;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Empty = EventSourcing.Contracts.Empty;

namespace EventSourcing.LockWriteService
{
    public class LockWriteService : LockWrite.LockWriteBase
    {
        private readonly RedisDataStore _redisDataStore;
        private readonly ILockProvider _lockProvider;
        private readonly KafkaProducer<string, Lock> _producer;

        public LockWriteService(KafkaProducer<string, Lock> producer, RedisDataStore redisDataStore, ILockProvider lockProvider)
        {
            _producer = producer;
            _redisDataStore = redisDataStore;
            _lockProvider = lockProvider;
        }

        public override async Task<Lock> LockResource(LockRequest request, ServerCallContext context)
        {
            await CheckLockStatus(request);
            var lockToken = await TakeLock(request);

            var vehicleLock = new Lock
            {
                ResourceId = request.ResourceId,
                LockHolderId = request.Requester,
                LockId = lockToken.LockValue,
                ResourceType = request.ResourceType,
                Expiry = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddSeconds(request.HoldSeconds))
            };

            await _redisDataStore.Set(vehicleLock, request.ResourceId);
            await _producer.ProduceAsync(vehicleLock, request.ResourceId);

            return vehicleLock;
        }

        public override async Task<Empty> ReleaseLock(Lock request, ServerCallContext context)
        {
            var currentLock = await _redisDataStore.Get<Lock>(request.ResourceId);

            if (currentLock.IsNullOrDefault())
                throw new RpcException(new Status(StatusCode.NotFound, $"No lock found for resource: {request.ResourceId}"), $"No lock found for resource: {request.ResourceId}");

            if (!currentLock.LockId.Equals(request.LockId, StringComparison.OrdinalIgnoreCase))
                throw new RpcException(new Status(StatusCode.Unavailable, $"Unable to release lock, the lockId: {request.LockId} is incorrect."),
                    $"Unable to release lock, the lockId: {request.LockId} is incorrect.");

            if (!await _lockProvider.ReleaseLock(new LockToken(true, currentLock.LockId, currentLock.ResourceId, currentLock.Expiry.ToDateTime())))
                throw new RpcException(new Status(StatusCode.Unavailable, "Unable to release lock."), "Unable to release lock.");

            await _redisDataStore.Delete<Lock>(currentLock.ResourceId);
            currentLock.Expiry = DateTime.UtcNow.AddSeconds(5).ToTimestamp();
            currentLock.Released = false;
            await _producer.ProduceAsync(currentLock, request.ResourceId);

            return new Empty();
        }

        private async Task CheckLockStatus(LockRequest request)
        {
            var currentLock = await _redisDataStore.Get<Lock>(request.ResourceId);
            if (currentLock.IsNotNullOrDefault() && !currentLock.Equals(new Lock()) && !currentLock.IsInactive())
            {
                var errorMessage = $"Resource is already locked by {currentLock.LockHolderId} until: {currentLock.Expiry}";
                throw new RpcException(new Status(StatusCode.AlreadyExists, errorMessage), errorMessage);
            }
        }

        private async Task<LockToken> TakeLock(LockRequest lockRequest)
        {
            var lockToken = await _lockProvider.TakeLock(lockRequest.ResourceId, lockRequest.HoldSeconds * 1000);
            if (!lockToken.GotLock)
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Resource is already locked."), "Resource is already locked.");

            return lockToken;
        }
    }
}