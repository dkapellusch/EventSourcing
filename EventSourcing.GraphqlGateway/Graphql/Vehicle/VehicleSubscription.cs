using System;
using EventSourcing.Contracts;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using static EventSourcing.Contracts.VehicleRead;

namespace EventSourcing.GraphqlGateway.Graphql.Vehicle
{
    public sealed class VehicleSubscription : ObjectGraphType<Contracts.Vehicle>
    {
        private readonly VehicleReadClient _vehicleReadClient;

        public VehicleSubscription(VehicleReadClient vehicleReadClient)
        {
            _vehicleReadClient = vehicleReadClient;

            AddField(new EventStreamFieldType
            {
                Name = "vehicleChange",
                Type = typeof(VehicleType),
                Resolver = new FuncFieldResolver<Contracts.Vehicle>(ctx => ctx.Source as Contracts.Vehicle),
                Subscriber = new EventStreamResolver<Contracts.Vehicle>(GetVehicleChangeSubscription)
            });
        }

        private IObservable<Contracts.Vehicle> GetVehicleChangeSubscription(ResolveEventStreamContext context) =>
            _vehicleReadClient.GetVehicleUpdates(new Empty())
                .ResponseStream
                .AsObservable();
    }
}