using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.VehicleReadService
{
    public class KsqlVehicleReadService : VehicleRead.VehicleReadBase
    {
        private readonly KafkaBackedDb<Vehicle> _db;

        public KsqlVehicleReadService(KafkaBackedDb<Vehicle> db) => _db = db;

        public override async Task<Vehicle> GetVehicle(VehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _db.Get(request.Vin);
            if (vehicle is null) throw new RpcException(new Status(StatusCode.NotFound, "No Dice"), "Not Found man");

            return vehicle;
        }

        public override async Task GetVehicleUpdates(Empty request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context) =>
            await _db.GetChanges().ForEachAsync(async v => await responseStream.WriteAsync(v), context.CancellationToken);
    }

    public class VehicleReadService : VehicleRead.VehicleReadBase
    {
        private readonly KafkaBackedDb<Vehicle> _db;

        public VehicleReadService(KafkaBackedDb<Vehicle> db) => _db = db;

        public override async Task<Vehicle> GetVehicle(VehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _db.Get(request.Vin);
            return vehicle;
        }

        public override async Task GetVehicleUpdates(Empty request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context) =>
            await _db.GetChanges().ForEachAsync(m => responseStream.WriteAsync(m), context.CancellationToken);
    }
}