using System.Threading.Tasks;
using EventSourcing.Contracts;
using Grpc.Core;
using static EventSourcing.Contracts.LocationRead;

namespace EventSourcing.GraphqlGateway.Graphql.Vehicle
{
    public class VehicleLocationResolver : IResolver<Contracts.Vehicle, Contracts.Location>
    {
        private readonly LocationReadClient _locationReadClient;

        public VehicleLocationResolver(LocationReadClient locationReadClient)
        {
            _locationReadClient = locationReadClient;
        }

        public async Task<Contracts.Location> Resolve(Contracts.Vehicle source)
        {
            try
            {
                return await _locationReadClient.GetLocationAsync(new LocationRequest {LocationCode = source.LocationCode});
            }
            catch (RpcException ex) when (ex.Status.StatusCode == StatusCode.NotFound)
            {
                return null;
            }
        }
    }
}