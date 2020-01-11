using System;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Rewrite;
using Empty = EventSourcing.Contracts.Empty;

namespace EventSourcing.LockWriteService
{
    public class LockWriteService : LockWrite.LockWriteBase
    {
        private readonly KafkaProducer<string, Lock> _producer;
        private readonly LockRead.LockReadClient _lockReadClient;

        public LockWriteService(KafkaProducer<string, Lock> producer, LockRead.LockReadClient lockReadClient)
        {
            _producer = producer;
            _lockReadClient = lockReadClient;
        }

        public override async Task<Lock> LockVehicle(LockRequest request, ServerCallContext context)
        {
            var currentLock = await _lockReadClient.GetLockAsync(request);
            if (currentLock.IsNotNullOrDefault() && !currentLock.Equals(new Lock()))
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Vehicle is already locked."), "Vehicle is already locked.");

            var vehicleLock = new Lock
            {
                Vin = request.Vin,
                Expiry = Timestamp.FromDateTimeOffset(DateTimeOffset.Now.AddSeconds(request.HoldSeconds)),
                LockId = Guid.NewGuid().ToString()
            };

            await _producer.ProduceAsync(vehicleLock, request.Vin);

            return vehicleLock;
        }
    }
}