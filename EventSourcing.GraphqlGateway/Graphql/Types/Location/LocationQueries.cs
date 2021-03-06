using EventSourcing.Contracts;
using GraphQL.Types;
using static EventSourcing.Contracts.LocationRead;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Location
{
    public class LocationQueries : ObjectGraphType
    {
        public LocationQueries(LocationReadClient locationReadClient)
        {
            FieldAsync<LocationType>("location",
                "a location",
                new QueryArguments(new QueryArgument(typeof(StringGraphType)) {Name = "code"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                    await locationReadClient.GetLocationAsync(
                        new LocationRequest
                        {
                            LocationCode = ctx.Arguments["code"].ToString()
                        }
                    )
                ));
        }
    }
}