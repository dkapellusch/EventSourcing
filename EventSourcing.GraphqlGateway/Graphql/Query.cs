using System.Threading.Tasks;
using EventSourcing.Contracts;
using HotChocolate;
using HotChocolate.Types;
using static EventSourcing.Contracts.LocationRead;
using static EventSourcing.Contracts.VehicleRead;
using Location = EventSourcing.Contracts.Location;

namespace EventSourcing.GraphqlGateway.Graphql;

public class Query
{
    public async Task<Vehicle?> GetVehicleAsync(string vin, [Service] VehicleReadClient client)
    {
        try
        {
            return await client.GetVehicleAsync(new VehicleRequest { Vin = vin });
        }
        catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Location?> GetLocationAsync(string code, [Service] LocationReadClient client)
    {
        try
        {
            return await client.GetLocationAsync(new LocationRequest { LocationCode = code });
        }
        catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<Contracts.Lock?> GetLockAsync(string resourceId, [Service] LockRead.LockReadClient client)
    {
        try
        {
            return await client.GetLockAsync(new LockRequest { ResourceId = resourceId });
        }
        catch (Grpc.Core.RpcException ex) when (ex.Status.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            return null;
        }
    }
}
