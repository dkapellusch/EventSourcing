using System.Threading.Tasks;
using EventSourcing.Contracts;
using HotChocolate;
using HotChocolate.Types;
using static EventSourcing.Contracts.LocationWrite;
using static EventSourcing.Contracts.VehicleWrite;
using Location = EventSourcing.Contracts.Location;

namespace EventSourcing.GraphqlGateway.Graphql;

public class Mutation
{
    public async Task<MutationResult> AddOrUpdateVehicleAsync(
        VehicleInput vehicle,
        [Service] VehicleWriteClient client)
    {
        var result = await client.AddVehicleAsync(new Vehicle
        {
            Vin = vehicle.Vin,
            Make = vehicle.Make ?? string.Empty,
            Model = vehicle.Model ?? string.Empty,
            LocationCode = vehicle.LocationCode ?? string.Empty
        });
        return new MutationResult { Id = result.Vin };
    }

    public async Task<MutationResult> AddOrUpdateLocationAsync(
        LocationInput location,
        [Service] LocationWriteClient client)
    {
        var result = await client.AddLocationAsync(new Location
        {
            LocationCode = location.LocationCode,
            LocationName = location.LocationName ?? string.Empty
        });
        return new MutationResult { Id = result.LocationCode };
    }

    public async Task<Contracts.Lock> TakeLockAsync(
        LockTakeInput lockInput,
        [Service] LockWrite.LockWriteClient client)
    {
        return await client.LockResourceAsync(new LockRequest
        {
            ResourceId = lockInput.ResourceId,
            Requester = lockInput.Requester ?? string.Empty,
            ResourceType = lockInput.ResourceType ?? string.Empty,
            HoldSeconds = lockInput.HoldSeconds
        });
    }

    public async Task<MutationResult> ReleaseLockAsync(
        LockReleaseInput lockInput,
        [Service] LockWrite.LockWriteClient client)
    {
        await client.ReleaseLockAsync(new Contracts.Lock
        {
            LockId = lockInput.LockId,
            ResourceId = lockInput.ResourceId
        });
        return new MutationResult { Id = lockInput.LockId };
    }
}

public record MutationResult
{
    public string Id { get; init; } = string.Empty;
}

public record VehicleInput(string Vin, string? Make, string? Model, string? LocationCode);

public record LocationInput(string LocationCode, string? LocationName);

public record LockTakeInput(string ResourceId, string? Requester, string? ResourceType, int HoldSeconds);

public record LockReleaseInput(string LockId, string ResourceId);
