using EventSourcing.Contracts;
using GraphQL.Types;
using static EventSourcing.Contracts.VehicleRead;

namespace EventSourcing.GraphqlGateway.Graphql.Vehicle
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

            FieldAsync<ListGraphType<VehicleType>>("vehiclesByPartialVin",
                "vehicles with partial vin",
                new QueryArguments(new QueryArgument(typeof(StringGraphType)) {Name = "vin"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var vehicles = await vehicleReadClient.GetVehiclesByPartialVinAsync(new VehicleRequest {Vin = ctx.Arguments["vin"].ToString()});
                    return vehicles.Elements;
                }));

            FieldAsync<ListGraphType<VehicleType>>("vehicles",
                "all vehicles",
                null,
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var response = await vehicleReadClient.GetAllVehiclesAsync(new Empty());
                    return response.Elements;
                }));
        }
    }
}