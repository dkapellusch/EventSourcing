using System;
using System.Threading;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.KSQL
{
    public class KsqlConsumer<TValue>
    {
        private readonly Mapper<TValue> _mapper;
        private readonly KsqlQuery _query;
        private readonly KsqlQueryExecutor _queryExecutor;

        public KsqlConsumer(KsqlQueryExecutor queryExecutor, Mapper<TValue> mapper, KsqlQuery query)
        {
            _queryExecutor = queryExecutor;
            _mapper = mapper;
            _query = query;
        }

        public IObservable<TValue> Subscription { get; private set; }

        public void Start(CancellationToken token = default)
        {
            if (Subscription != null) return;

            Subscription = _queryExecutor.ExecuteQuery(_query, _mapper, token).AsObservable();
        }
    }
}