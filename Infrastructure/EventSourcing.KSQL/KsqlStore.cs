using System;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.DataStore;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.KSQL
{
    public class KsqlStore<TValue> : IReadonlyDataStore<TValue>, IChangeTracking<TValue>
    {
        private readonly KsqlQueryExecutor _queryExecutor;
        private readonly Mapper<TValue> _mapper;
        private readonly string _tableName;
        private readonly KsqlConsumer<TValue> _consumer;

        public KsqlStore(KsqlQueryExecutor queryExecutor, Mapper<TValue> mapper, string stream, string table = null)
        {
            table ??= stream;
            _queryExecutor = queryExecutor;
            _mapper = mapper;
            _tableName = table;
            _consumer = new KsqlConsumer<TValue>(_queryExecutor,
                mapper,
                new KsqlQuery
                {
                    Ksql = $"Select * from {stream} emit changes;"
                });
            _consumer.Start();
        }

        public async Task<TValue> Get(string key) => (await _queryExecutor.ExecuteQuery(new KsqlQuery
                    {
                        Ksql = $"Select * from {_tableName} where rowkey = '{key}';"
                    },
                    _mapper)
                .EnumerateAsync())
            .LastOrDefault();

        public IObservable<TValue> GetChanges() => _consumer.Subscription;
    }
}