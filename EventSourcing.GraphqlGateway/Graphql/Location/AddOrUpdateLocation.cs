using GraphQL.Types;
using static EventSourcing.Contracts.LocationWrite;

namespace EventSourcing.GraphqlGateway.Graphql.Location
{
    public class AddOrUpdateLocation : ObjectGraphType
    {
        public AddOrUpdateLocation(LocationWriteClient locationService)
        {
            FieldAsync<MutationResultType>(GetType().Name,
                "Add or update a location",
                new QueryArguments(new QueryArgument<NonNullGraphType<LocationInputType>> {Name = "location"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var inputLocation = ctx.GetArgument<Contracts.Location>("location");
                    var changedLocation = await locationService.AddLocationAsync(inputLocation);

                    return new MutationResult {Id = changedLocation.LocationCode};
                }));
        }
    }
}