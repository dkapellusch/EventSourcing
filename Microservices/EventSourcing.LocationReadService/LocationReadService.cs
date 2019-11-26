using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.LocationReadService
{
    public class LocationReadService : LocationRead.LocationReadBase
    {
        private readonly KafkaBackedDb<Location> _db;

        public LocationReadService(KafkaBackedDb<Location> db)
        {
            _db = db;
        }

        public override Task<Location> GetLocation(LocationRequest request, ServerCallContext context) =>
            Task.FromResult(
                _db.GetItem(request.LocationCode)
                ?? throw new RpcException(new Status(StatusCode.NotFound, $"No location found for code: {request.LocationCode}."))
            );

        public override Task<Locations> GetAllLocations(Empty request, ServerCallContext context) => Task.FromResult(new Locations {Elements = {_db.GetAll().ToArray()}});
    }
}