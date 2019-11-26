using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using Grpc.Core;

namespace EventSourcing.VehicleReadService
{
    public class VehicleReadService : VehicleRead.VehicleReadBase
    {
        private readonly KafkaBackedDb<Vehicle> _db;

        public VehicleReadService(KafkaBackedDb<Vehicle> db)
        {
            _db = db;
        }

        public override Task<Vehicle> GetVehicle(VehicleRequest request, ServerCallContext context)
        {
            var result = _db.GetItem(request.Vin);
            return Task.FromResult(result);
        }

        public override Task<Vehicles> GetVehiclesByPartialVin(VehicleRequest request, ServerCallContext context)
        {
            var partialVin = request.Vin;
            var vehicleMatches = _db.GetItems(partialVin).ToArray();
            var vehicles = new Vehicles {Elements = {vehicleMatches}};
            return Task.FromResult(vehicles);
        }

        public override Task<Vehicles> GetAllVehicles(Empty request, ServerCallContext context) => Task.FromResult(new Vehicles {Elements = {_db.GetAll()}});

        public override async Task GetVehicleUpdates(Empty request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context) =>
            await _db.GetChanges().ForEachAsync(m => responseStream.WriteAsync(m), context.CancellationToken);
    }
}