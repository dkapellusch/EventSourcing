using System;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.LockReadService
{
    public class LockReadService : LockRead.LockReadBase
    {
        private readonly KafkaBackedDb<Lock> _db;

        public LockReadService(KafkaBackedDb<Lock> db) => _db = db;

        public override Task<Lock> GetLock(LockRequest request, ServerCallContext context)
        {
            var currentLock = _db.GetItem(request.Vin);
            if (currentLock.IsNullOrDefault() || currentLock.Expiry.ToDateTime() < DateTime.UtcNow) currentLock = new Lock();

            return Task.FromResult(currentLock);
        }
    }
}