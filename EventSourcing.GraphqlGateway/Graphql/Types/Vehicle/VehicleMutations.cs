using GraphQL.Types;
using static EventSourcing.Contracts.VehicleWrite;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Vehicle
{
    public class VehicleMutations : ObjectGraphType
    {
        public VehicleMutations(VehicleWriteClient vehicleWriteClient)
        {
            FieldAsync<MutationResultType>("AddOrUpdateVehicle",
                "Add or update a vehicle",
                new QueryArguments(new QueryArgument<NonNullGraphType<VehicleInputType>> {Name = "vehicle"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var inputVehicle = ctx.GetArgument<Contracts.Vehicle>("vehicle");
                    var changedVehicle = await vehicleWriteClient.AddVehicleAsync(inputVehicle);
                    return new MutationResult {Id = changedVehicle.Vin};
                })
            );
        }
    }
}