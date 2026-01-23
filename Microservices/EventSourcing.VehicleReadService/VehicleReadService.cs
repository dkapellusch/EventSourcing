using System.Collections.Generic;
using System.Linq;
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

        public override async Task<VehicleList> GetVehiclesAtLocation(LocationVehiclesRequest request, ServerCallContext context)
        {
            var allVehicles = await _db.Query();
            var filtered = allVehicles.Where(v => v.LocationCode == request.LocationCode);
            var result = new VehicleList();
            result.Vehicles.AddRange(filtered);
            return result;
        }

        public override async Task GetVehicleUpdatesAtLocation(LocationVehiclesRequest request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context) =>
            await _db.GetChanges()
                .Where(v => v.LocationCode == request.LocationCode)
                .ForEachAsync(v => responseStream.WriteAsync(v), context.CancellationToken);
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

        public override async Task<VehicleList> GetVehiclesAtLocation(LocationVehiclesRequest request, ServerCallContext context)
        {
            var allVehicles = await _db.Query();
            var filtered = allVehicles.Where(v => v.LocationCode == request.LocationCode);
            var result = new VehicleList();
            result.Vehicles.AddRange(filtered);
            return result;
        }

        public override async Task GetVehicleUpdatesAtLocation(LocationVehiclesRequest request, IServerStreamWriter<Vehicle> responseStream, ServerCallContext context) =>
            await _db.GetChanges()
                .Where(v => v.LocationCode == request.LocationCode)
                .ForEachAsync(v => responseStream.WriteAsync(v), context.CancellationToken);
    }
}