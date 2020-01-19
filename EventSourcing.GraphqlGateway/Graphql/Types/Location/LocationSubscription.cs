using System;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Location
{
    public sealed class LocationSubscription : ObjectGraphType<Contracts.Location>
    {
        private readonly LocationRead.LocationReadClient _locationReadClient;

        public LocationSubscription(LocationRead.LocationReadClient locationReadClient)
        {
            _locationReadClient = locationReadClient;

            AddField(new EventStreamFieldType
            {
                Name = "locationChange",
                Type = typeof(LocationType),
                Resolver = new FuncFieldResolver<Contracts.Location>(ctx => ctx.Source as Contracts.Location),
                Subscriber = new EventStreamResolver<Contracts.Location>(GetLocationChangeSubscription)
            });
        }

        private IObservable<Contracts.Location> GetLocationChangeSubscription(ResolveEventStreamContext context) =>
            _locationReadClient.GetLocationUpdates(new Empty())
                .ResponseStream
                .AsObservable();
    }
}