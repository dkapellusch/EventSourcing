using EventSourcing.GraphqlGateway.Graphql.Types.Location;
using EventSourcing.GraphqlGateway.Graphql.Types.Lock;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Vehicle
{
    public class VehicleType : ObjectGraphType<Contracts.Vehicle>
    {
        public VehicleType(IResolver<Contracts.Vehicle, Contracts.Location> locationResolver, IResolver<Contracts.Vehicle, Contracts.Lock> lockResolver)
        {
            Field<IdGraphType>(nameof(Contracts.Vehicle.Vin));
            Field(v => v.Make);
            Field(v => v.Model);
            Field<LocationType>("location",
                "a location",
                resolve: ctx => ctx.TryAsyncResolve(async context => await locationResolver.Resolve(context.Source))
            );

            Field<LockType>("lock", "a lock", resolve: ctx => ctx.TryAsyncResolve(async context => await lockResolver.Resolve(context.Source)));
        }
    }

    public class VehicleInputType : InputObjectGraphType<Contracts.Vehicle>
    {
        public VehicleInputType()
        {
            Field<IdGraphType>(nameof(Contracts.Vehicle.Vin));
            Field(v => v.Make, true);
            Field(v => v.Model, true);
            Field(v => v.LocationCode, true);
        }
    }
}