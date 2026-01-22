using System.Threading.Tasks;
using EventSourcing.Contracts;
using HotChocolate;
using HotChocolate.Types;
using static EventSourcing.Contracts.LocationRead;
using Location = EventSourcing.Contracts.Location;

namespace EventSourcing.GraphqlGateway.Graphql.Types;

[ExtendObjectType(typeof(Vehicle))]
public class VehicleTypeExtension
{
    public async Task<Location?> GetLocationAsync(
        [Parent] Vehicle vehicle,
        [Service] LocationReadClient client)
    {
        if (string.IsNullOrEmpty(vehicle.LocationCode))
            return null;

        try
        {
            return await client.GetLocationAsync(new LocationRequest { LocationCode = vehicle.LocationCode });
        }
        catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Contracts.Lock?> GetLockAsync(
        [Parent] Vehicle vehicle,
        [Service] LockRead.LockReadClient client)
    {
        try
        {
            return await client.GetLockAsync(new LockRequest
            {
                ResourceId = vehicle.Vin,
                ResourceType = nameof(Vehicle)
            });
        }
        catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }
}
