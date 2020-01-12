using System;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Kafka;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace EventSourcing.LockWriteService
{
    public class LockWriteService : LockWrite.LockWriteBase
    {
        private readonly KafkaProducer<string, Lock> _producer;
        private readonly LockRead.LockReadClient _lockReadClient;
        private readonly IExpiringDataStore _expiringDataStore;

        public LockWriteService(KafkaProducer<string, Lock> producer, LockRead.LockReadClient lockReadClient, IExpiringDataStore expiringDataStore)
        {
            _producer = producer;
            _lockReadClient = lockReadClient;
            _expiringDataStore = expiringDataStore;
        }

        public override async Task<Lock> LockResource(LockRequest request, ServerCallContext context)
        {
            await CheckLockStatus(request);

            var vehicleLock = new Lock
            {
                ResourceId = request.ResourceId,
                LockHolderId = request.Requester,
                LockId = Guid.NewGuid().ToString(),
                Expiry = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(request.HoldSeconds)),
                Released = false
            };

            await _expiringDataStore.Set(vehicleLock, request.ResourceId, TimeSpan.FromSeconds(request.HoldSeconds));
            await _producer.ProduceAsync(vehicleLock, request.ResourceId);

            return vehicleLock;
        }

        private async Task CheckLockStatus(LockRequest request)
        {
            var currentLock = await _lockReadClient.GetLockAsync(request);
            if (currentLock.IsNotNullOrDefault() && !currentLock.Equals(new Lock()))
                throw new RpcException(
                    new Status(StatusCode.AlreadyExists, $"Resource is already locked by {currentLock.LockHolderId}"),
                    $"Resource is already locked by {currentLock.LockHolderId}"
                );
        }
    }
}