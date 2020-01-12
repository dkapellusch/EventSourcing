using System.Linq;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using static EventSourcing.Contracts.VehicleRead;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Location
{
    public class LocationVehicleResolver : IResolver<Contracts.Location, Contracts.Vehicle[]>
    {
        private readonly VehicleReadClient _vehicleReadClient;

        public LocationVehicleResolver(VehicleReadClient vehicleReadClient) => _vehicleReadClient = vehicleReadClient;

        public async Task<Contracts.Vehicle[]> Resolve(Contracts.Location source)
        {
            var vehicles = await _vehicleReadClient.GetAllVehiclesAsync(new Empty());
            return vehicles.Elements.Where(v => v.LocationCode == source.LocationCode).ToArray();
        }
    }
}