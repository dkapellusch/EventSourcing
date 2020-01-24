using System;
using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.KSQL
{
    public class KsqlStore<TValue>
    {
        private readonly KsqlConsumer<TValue> _consumer;
        private readonly Mapper<TValue> _mapper;
        private readonly KsqlQuery _tableQuery;
        private readonly KsqlQueryExecutor _queryExecutor;

        public KsqlStore(KsqlQueryExecutor queryExecutor, Mapper<TValue> mapper, KsqlQuery changeQuery, KsqlQuery tableQuery)
        {
            _queryExecutor = queryExecutor;
            _mapper = mapper;
            _tableQuery = tableQuery;
            _consumer = new KsqlConsumer<TValue>(_queryExecutor, mapper, changeQuery);
            _consumer.Start();
        }

        public IObservable<TValue> GetChanges() => _consumer.Subscription;

        public async Task<TValue> Get(string key) =>
            (await _queryExecutor.ExecuteQuery(new KsqlQuery
                    {
                        Ksql = string.Format(_tableQuery.Ksql, key),
                        StreamProperties = _tableQuery.StreamProperties
                    },
                    _mapper)
                .EnumerateAsync())
            .LastOrDefault();
    }
}