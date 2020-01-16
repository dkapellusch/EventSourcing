using System.Threading.Tasks;
using EventSourcing.Contracts;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Vehicle
{
    public class VehicleLockResolver : IResolver<Contracts.Vehicle, Contracts.Lock>
    {
        private readonly LockRead.LockReadClient _lockReadClient;

        public VehicleLockResolver(LockRead.LockReadClient lockReadClient) => _lockReadClient = lockReadClient;

        public async Task<Contracts.Lock> Resolve(Contracts.Vehicle source) =>
            await _lockReadClient.GetLockAsync(new LockRequest {ResourceId = source.Vin, ResourceType = typeof(Contracts.Vehicle).Name});
    }
}