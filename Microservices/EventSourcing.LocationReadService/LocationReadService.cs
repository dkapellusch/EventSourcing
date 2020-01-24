using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.LocationReadService
{
    public class LocationRequestHandler
    {
        private readonly KafkaBackedDb<Location> _db;

        public LocationRequestHandler(KafkaBackedDb<Location> db) => _db = db;

        public IObservable<Location> GetLocationPipeLine(LocationRequest request) =>
            Observable.Return(request)
                .Do(Console.WriteLine)
                .Select(r => { return Observable.FromAsync(async t => await _db.Get(request.LocationCode)); })
                .Concat()
                .Do(Console.WriteLine);

        public IObservable<Notification<Location>> GetLocationUpdates() =>
            Observable.Empty<Location>()
                .Do(Console.WriteLine)
                .Select(r => _db.GetChanges())
                .Concat()
                .Materialize()
                .Do(Console.WriteLine);
    }

    public class LocationReadService : LocationRead.LocationReadBase
    {
        private readonly LocationRequestHandler _locationRequestHandler;

        public LocationReadService(LocationRequestHandler locationRequestHandler) => _locationRequestHandler = locationRequestHandler;

        public override async Task<Location> GetLocation(LocationRequest request, ServerCallContext context) =>
            await _locationRequestHandler.GetLocationPipeLine(request);

        public override async Task GetLocationUpdates(Empty request, IServerStreamWriter<Location> responseStream, ServerCallContext context) =>
            await _locationRequestHandler.GetLocationUpdates().ForEachAsync(message => responseStream.WriteAsync(message.Value), context.CancellationToken);
    }
}