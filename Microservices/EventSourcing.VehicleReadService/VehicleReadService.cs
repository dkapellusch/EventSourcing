using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.KSQL;
using Grpc.Core;

namespace EventSourcing.VehicleReadService
{
    public class KsqlVehicleReadService : VehicleRead.VehicleReadBase
    {
        private readonly VehicleKsqlTable _vehicleKsqlTable;

        public KsqlVehicleReadService(VehicleKsqlTable vehicleKsqlTable) => _vehicleKsqlTable = vehicleKsqlTable;

        public override async Task<Vehicle> GetVehicle(VehicleRequest request, ServerCallContext context)
        {
            var vehicle = await _vehicleKsqlTable.GetVehicleByVinAsync(request.Vin);
            if (vehicle is null) throw new RpcException(new Status(StatusCode.NotFound, "No Dice"), "Not Found man");

            return vehicle;
        }

        public override async Task<Vehicles> GetVehiclesByPartialVin(VehicleRequest request, ServerCallContext context)
        {
            var partialVin = request.Vin;
            var allVehicles = await _vehicleKsqlTable.GetAllVehiclesAsync();
            return new Vehicles {Elements = {allVehicles.Where(v => v.Vin.StartsWith(partialVin, StringComparison.OrdinalIgnoreCase))}};
        }

        public override async Task<Vehicles> GetAllVehicles(Empty request, ServerCallContext context) => new Vehicles {Elements = {await _vehicleKsqlTable.GetAllVehiclesAsync()}};

        public override async Task GetVehicleUpdates(Empty request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context)
        {
            await foreach (var vehicle in _vehicleKsqlTable.GetVehicleUpdatesAsync(context.CancellationToken)) await responseStream.WriteAsync(vehicle);
        }
    }

    public class VehicleReadService : VehicleRead.VehicleReadBase
    {
        private readonly KafkaBackedDb<Vehicle> _db;

        public VehicleReadService(KafkaBackedDb<Vehicle> db) => _db = db;

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