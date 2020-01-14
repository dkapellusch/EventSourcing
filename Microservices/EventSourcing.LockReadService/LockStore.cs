using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;
using EventSourcing.KSQL;
using Google.Protobuf.WellKnownTypes;

namespace EventSourcing.LockReadService
{
    public class LockStore : IReadonlyDataStore<Lock>, IChangeTracking<Lock>
    {
        private readonly KsqlQueryExecutor _queryExecutor;

        private const string CreateActiveLockTable =
            @"create table ActiveLocks_By_ResourceId as 
            select 
                   resourceId, 
                   count(resourceId) as lockedCount,
                   collect_set(LOCKHOLDERID) as LOCKHOLDERID, 
                   collect_set(RESOURCETYPE) as RESOURCETYPE , 
                   collect_set(lockId) as lockId, 
                   collect_set(expiry) as expiry, 
                   collect_set(released) as released, 
                   collect_set(Inactive) as Inactive 
            from ActiveLocks group by resourceId;";

        public LockStore(KsqlQueryExecutor queryExecutor) => _queryExecutor = queryExecutor;

        public async Task<Lock> Get(string key)
        {
            var activeLock = (await _queryExecutor.ExecuteQuery(new KsqlQuery
                        {
                            Ksql = $"Select * from ActiveLocks_By_ResourceId where rowkey = '{key}';"
                        },
                        ParseLock)
                    .EnumerateAsync())
                .FirstOrDefault(l => !l.IsInactive());

            var inactiveLock = (await _queryExecutor.ExecuteQuery(new KsqlQuery
                        {
                            Ksql = $"Select * from InactiveLocks_By_ResourceId where rowkey = '{key}';"
                        },
                        ParseLock)
                    .EnumerateAsync())
                .FirstOrDefault(l => l?.LockId == activeLock?.LockId);

            return inactiveLock is null ? activeLock : null;
        }

        public IObservable<Lock> GetChanges() =>
            _queryExecutor.ExecuteQuery(new KsqlQuery
                    {
                        Ksql = "Select * from INACTIVELOCKS_BY_RESOURCEID emit changes;"
                    },
                    ParseLock)
                .AsObservable();

        private static Lock ParseLock(IDictionary<string, dynamic> columns) =>
            new Lock
            {
                LockId = columns.GetValue<Lock, string>(l => l.LockId),
                ResourceId = columns.GetValue<Lock, string>(l => l.ResourceId),
                ResourceType = columns.GetValue<Lock, string>(l => l.ResourceType),
                LockHolderId = columns.GetValue<Lock, string>(l => l.LockHolderId),
                Released = columns.GetValue<Lock, bool>(l => l.Released),
                Expiry = columns.GetValue<Lock, Timestamp>(l => l.Expiry,
                    s => DateTime.Parse(s)
                        .ToUniversalTime()
                        .AddHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours)
                        .ToTimestamp())
            };
    }
}