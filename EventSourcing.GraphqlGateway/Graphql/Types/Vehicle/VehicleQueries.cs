using EventSourcing.Contracts;
using GraphQL.Types;
using static EventSourcing.Contracts.VehicleRead;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Vehicle
{
    public class VehicleQueries : ObjectGraphType<VehicleType>
    {
        public VehicleQueries(VehicleReadClient vehicleReadClient)
        {
            Name = "Vehicle";

            FieldAsync<VehicleType>("vehicle",
                "a vehicle",
                new QueryArguments(new QueryArgument(typeof(StringGraphType)) {Name = "vin"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                    await vehicleReadClient.GetVehicleAsync(new VehicleRequest {Vin = ctx.Arguments["vin"].ToString()}))
            );
        }
    }
}