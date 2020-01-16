using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
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
                // .Select(r => Observable.FromAsync(async t => await _db.Get(request.LocationCode)))
                .Select(r =>
                {
                    // if (DateTime.UtcNow.Millisecond % 2 == 0)
                    //     throw new InvalidCastException("Coin was heads.");

                    return Observable.FromAsync(async t => await _db.Get(request.LocationCode));
                })
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
        private LocationRequestHandler _locationRequestHandler;
        // private readonly KafkaBackedDb<Location> _db;
        //
        // public LocationReadService(KafkaBackedDb<Location> db) => _db = db;

        public LocationReadService(LocationRequestHandler locationRequestHandler) => _locationRequestHandler = locationRequestHandler;

        public override async Task<Location> GetLocation(LocationRequest request, ServerCallContext context)
        {
            var pipeLine = await _locationRequestHandler.GetLocationPipeLine(request);
            // var result = await pipeLine.FirstOrDefaultAsync();
            // if (result.Exception.IsNotNullOrDefault()) throw new RpcException(new Status(StatusCode.Internal, result.Exception.Message), result.Exception.Message);

            return pipeLine;
        }

        public override async Task GetLocationUpdates(Empty request, IServerStreamWriter<Location> responseStream, ServerCallContext context) =>
            await _locationRequestHandler.GetLocationUpdates().ForEachAsync(message => responseStream.WriteAsync(message.Value), context.CancellationToken);


        // public override async Task GetLocationUpdates(Empty request, IServerStreamWriter<Location> responseStream, ServerCallContext context) =>
        //     await _db.GetChanges().ForEachAsync(message => responseStream.WriteAsync(message), context.CancellationToken);
    }
}