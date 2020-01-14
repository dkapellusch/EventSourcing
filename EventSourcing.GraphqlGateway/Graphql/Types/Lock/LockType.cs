using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Lock
{
    public class LockType : ObjectGraphType<Contracts.Lock>
    {
        public LockType()
        {
            Field<IdGraphType>(nameof(Contracts.Lock.LockId));
            Field(l => l.ResourceId);
            Field(l => l.ResourceType);
            Field(l => l.LockHolderId);
            Field<DateTimeGraphType>(nameof(Contracts.Lock.Expiry));
            Field(l => l.Released);
        }
    }

    public class LockInputType : InputObjectGraphType<Contracts.Lock>
    {
        public LockInputType()
        {
            Field<IdGraphType>(nameof(Contracts.LockRequest.ResourceId));
            Field(typeof(StringGraphType), nameof(Contracts.LockRequest.ResourceType));
            Field(typeof(IntGraphType), nameof(Contracts.LockRequest.HoldSeconds));
        }
    }
}