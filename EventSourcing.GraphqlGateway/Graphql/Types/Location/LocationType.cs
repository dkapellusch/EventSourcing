using EventSourcing.GraphqlGateway.Graphql.Types.Vehicle;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Location
{
    public class LocationType : ObjectGraphType<Contracts.Location>
    {
        public LocationType(IResolver<Contracts.Location, Contracts.Vehicle[]> vehicleResolver)
        {
            Field<IdGraphType>(nameof(Contracts.Location.LocationCode));
            Field(l => l.LocationName);
            Field<ListGraphType<VehicleType>>(
                "vehicles",
                "a location",
                resolve: ctx => vehicleResolver.Resolve(ctx.Source)
            );
        }
    }

    public class LocationInputType : InputObjectGraphType<Contracts.Location>
    {
        public LocationInputType()
        {
            Field<IdGraphType>(nameof(Contracts.Location.LocationCode));
            Field(l => l.LocationName, true);
        }
    }
}