using System;
using System.Threading;
using EventSourcing.Contracts.Extensions;

namespace EventSourcing.KSQL
{
    public class KsqlConsumer<TValue>
    {
        private readonly KsqlQueryExecutor _queryExecutor;
        private readonly Mapper<TValue> _mapper;
        private readonly KsqlQuery _query;

        public KsqlConsumer(KsqlQueryExecutor queryExecutor, Mapper<TValue> mapper, KsqlQuery query)
        {
            _queryExecutor = queryExecutor;
            _mapper = mapper;
            _query = query;
        }

        public void Start(CancellationToken token = default)
        {
            if (Subscription != null) return;

            Subscription = _queryExecutor.ExecuteQuery(_query, _mapper, token).AsObservable();
        }

        public IObservable<TValue> Subscription { get; private set; }
    }
}