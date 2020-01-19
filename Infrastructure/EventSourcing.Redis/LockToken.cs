using System;

namespace EventSourcing.Redis
{
    public struct LockToken
    {
        public LockToken(bool gotLock, string lockValue, string lockKey, DateTime lockExpiry)
        {
            GotLock = gotLock;
            LockValue = lockValue;
            LockKey = lockKey;
            LockExpiry = lockExpiry;
        }

        public bool GotLock { get; }

        public string LockValue { get; }

        public string LockKey { get; }

        public DateTime LockExpiry { get; }
    }
}