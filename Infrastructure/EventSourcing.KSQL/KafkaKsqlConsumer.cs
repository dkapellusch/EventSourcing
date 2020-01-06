using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourcing.KSQL
{
    public sealed class KafkaKsqlConsumer<TRow>
    {
        private readonly KafkaKsqlQueryExecutor _queryExecutor;
        private readonly KsqlQuery _query;
        private readonly Mapper<TRow> _mapper;

        private Subject<TRow> _streamSubject;

        public KafkaKsqlConsumer(KafkaKsqlQueryExecutor queryExecutor, KsqlQuery query, Mapper<TRow> mapper)
        {
            _queryExecutor = queryExecutor;
            _query = query;
            _mapper = mapper;
        }

        public IObservable<TRow> Subscription => _streamSubject?.AsObservable();

        public void Start(CancellationToken token)
        {
            if (_streamSubject != null)
                return;

            _streamSubject = new Subject<TRow>();
            _ = ConsumeAsync(token);
        }

        private Task ConsumeAsync(CancellationToken token) =>
            Task.Run(async () =>
                {
                    await foreach (var row in _queryExecutor.ExecuteQuery(_query, _mapper).WithCancellation(token))
                    {
                        _streamSubject.OnNext(row);
                    }
                },
                token);
    }
}