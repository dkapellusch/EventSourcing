using System;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using EventSourcing.Kafka;
using EventSourcing.Redis;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventSourcing.LockWriteService
{
    public class LockWriteService : LockWrite.LockWriteBase
    {
        private readonly KafkaProducer<string, Lock> _producer;
        private readonly IExpiringDataStore _expiringDataStore;
        private readonly ILockProvider _lockProvider;

        public LockWriteService(KafkaProducer<string, Lock> producer, IExpiringDataStore expiringDataStore, ILockProvider lockProvider)
        {
            _producer = producer;
            _expiringDataStore = expiringDataStore;
            _lockProvider = lockProvider;
        }

        public override async Task<Lock> LockResource(LockRequest request, ServerCallContext context)
        {
            await CheckLockStatus(request);
            await GetLock(request);

            var vehicleLock = new Lock
            {
                ResourceId = request.ResourceId,
                LockHolderId = request.Requester,
                LockId = Guid.NewGuid().ToString(),
                ResourceType = request.ResourceType,
                Expiry = Timestamp.FromDateTimeOffset(DateTimeOffset.UtcNow.AddSeconds(request.HoldSeconds)),
                Released = false
            };

            await _expiringDataStore.Set(vehicleLock, request.ResourceId);
            await _producer.ProduceAsync(vehicleLock, request.ResourceId);

            return vehicleLock;
        }

        private async Task CheckLockStatus(LockRequest request)
        {
            var currentLock = await _expiringDataStore.Get<Lock>(request.ResourceId);
            if (currentLock.IsNotNullOrDefault() && !currentLock.Equals(new Lock()) && !currentLock.IsInactive())
            {
                var errorMessage = $"Resource is already locked by {currentLock.LockHolderId} until: {currentLock.Expiry}";
                throw new RpcException(
                    new Status(StatusCode.AlreadyExists, errorMessage),
                    errorMessage
                );
            }
        }

        private async Task GetLock(LockRequest lockRequest)
        {
            var lockToken = await _lockProvider.TakeLockAsync(lockRequest.ResourceId, lockRequest.HoldSeconds * 1000);
            if (!lockToken.GotLock)
                throw new RpcException(
                    new Status(StatusCode.AlreadyExists, "Resource is already locked."),
                    "Resource is already locked."
                );
        }
    }
}