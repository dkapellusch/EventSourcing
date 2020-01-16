using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Location
{
    public class LocationType : ObjectGraphType<Contracts.Location>
    {
        public LocationType()
        {
            Field<IdGraphType>(nameof(Contracts.Location.LocationCode));
            Field(l => l.LocationName);
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