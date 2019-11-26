using EventSourcing.Contracts;
using GraphQL.Types;
using static EventSourcing.Contracts.LocationRead;

namespace EventSourcing.GraphqlGateway.Graphql.Location
{
    public class LocationQueries : ObjectGraphType
    {
        public LocationQueries(LocationReadClient locationReadClient)
        {
            FieldAsync<LocationType>("location",
                "a location",
                new QueryArguments(new QueryArgument(typeof(StringGraphType)) {Name = "code"}),
                async ctx =>
                    await locationReadClient.GetLocationAsync(
                        new LocationRequest
                        {
                            LocationCode = ctx.Arguments["code"].ToString()
                        }
                    )
            );

            FieldAsync<ListGraphType<LocationType>>("locations",
                "all locations",
                null,
                async ctx =>
                {
                    var response = await locationReadClient.GetAllLocationsAsync(new Empty());
                    return response.Elements;
                });
        }
    }
}