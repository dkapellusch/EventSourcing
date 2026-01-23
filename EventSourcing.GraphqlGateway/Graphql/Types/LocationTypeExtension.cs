using System.Collections.Generic;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using HotChocolate;
using HotChocolate.Types;
using static EventSourcing.Contracts.VehicleRead;
using Location = EventSourcing.Contracts.Location;

namespace EventSourcing.GraphqlGateway.Graphql.Types;

[ExtendObjectType(typeof(Location))]
public class LocationTypeExtension
{
    public async Task<IEnumerable<Vehicle>> GetVehiclesAsync(
        [Parent] Location location,
        [Service] VehicleReadClient client)
    {
        var result = await client.GetVehiclesAtLocationAsync(
            new LocationVehiclesRequest { LocationCode = location.LocationCode });
        return result.Vehicles;
    }
}
