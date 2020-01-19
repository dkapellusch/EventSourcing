using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace EventSourcing.Redis
{
    public class RedisLockProvider : ILockProvider
    {
        private readonly IDatabaseAsync _redisDatabase;

        public RedisLockProvider(IDatabaseAsync redisDatabase) => _redisDatabase = redisDatabase;

        public async Task<LockToken> TakeLock(string lockKey, long lockHoldTimeMs)
        {
            try
            {
                var lockValue = Guid.NewGuid().ToString();
                var expiry = DateTime.UtcNow.AddMilliseconds(lockHoldTimeMs);
                var gotLock = await _redisDatabase.LockTakeAsync(GetKey(lockKey), lockValue, TimeSpan.FromMilliseconds(lockHoldTimeMs));

                return new LockToken(gotLock, lockValue, GetKey(lockKey), gotLock ? expiry : DateTime.MinValue);
            }
            catch
            {
                return new LockToken(false, string.Empty, GetKey(lockKey), DateTime.MinValue);
            }
        }

        public async Task<bool> ReleaseLock(LockToken lockToken)
        {
            if (!lockToken.GotLock) return false;

            try
            {
                var releasedLock = await _redisDatabase.LockReleaseAsync(GetKey(lockToken.LockKey), lockToken.LockValue);
                return releasedLock;
            }
            catch
            {
                return false;
            }
        }

        private static string GetKey(string key) => $"locks/{key}".ToLowerInvariant();
    }

    public interface ILockProvider
    {
        Task<LockToken> TakeLock(string lockKey, long lockHoldTimeMs);

        Task<bool> ReleaseLock(LockToken lockToken);
    }
}