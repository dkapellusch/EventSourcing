using EventSourcing.Contracts;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Lock
{
    public class LockMutations : ObjectGraphType
    {
        public LockMutations(LockWrite.LockWriteClient lockWriteClient)
        {
            FieldAsync<LockType>("TakeLock",
                "Take a lock",
                new QueryArguments(new QueryArgument<NonNullGraphType<LockTakeInputType>> {Name = "lock"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var lockRequest = ctx.GetArgument<LockRequest>("lock");
                    return await lockWriteClient.LockResourceAsync(lockRequest);
                })
            );

            FieldAsync<MutationResultType>("ReleaseLock",
                "Take a lock",
                new QueryArguments(new QueryArgument<NonNullGraphType<LockReleaseInputType>> {Name = "lock"}),
                async ctx => await ctx.TryAsyncResolve(async context =>
                {
                    var lockRequest = ctx.GetArgument<Contracts.Lock>("lock");
                    await lockWriteClient.ReleaseLockAsync(lockRequest);
                    return new MutationResult {Id = lockRequest.LockId};
                })
            );
        }
    }
}