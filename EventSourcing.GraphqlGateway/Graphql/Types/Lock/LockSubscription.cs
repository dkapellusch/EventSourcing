using System;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Lock
{
    public sealed class LockSubscription : ObjectGraphType<Contracts.Lock>
    {
        private readonly LockRead.LockReadClient _lockReadClient;

        public LockSubscription(LockRead.LockReadClient lockReadClient)
        {
            _lockReadClient = lockReadClient;

            AddField(new EventStreamFieldType
            {
                Name = "lockExpire",
                Type = typeof(LockType),
                Resolver = new FuncFieldResolver<Contracts.Lock>(ctx => ctx.Source as Contracts.Lock),
                Subscriber = new EventStreamResolver<Contracts.Lock>(GetLockChangeSubscription)
            });
        }

        private IObservable<Contracts.Lock> GetLockChangeSubscription(ResolveEventStreamContext context) =>
            _lockReadClient.ExpiringLocks(new Empty())
                .ResponseStream
                .AsObservable();
    }
}