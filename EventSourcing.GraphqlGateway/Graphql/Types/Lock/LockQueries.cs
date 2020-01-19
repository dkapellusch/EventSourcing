using EventSourcing.Contracts;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql.Types.Lock
{
    public class LockQueries : ObjectGraphType
    {
        public LockQueries(LockRead.LockReadClient lockReadClient)
        {
            FieldAsync<LockType>(nameof(Contracts.Lock),
                "a lock",
                new QueryArguments(new QueryArgument(typeof(StringGraphType)) {Name = "resourceId"}
                ),
                async ctx => await ctx.TryAsyncResolve(async context =>
                    await lockReadClient.GetLockAsync(
                        new LockRequest
                        {
                            ResourceId = ctx.Arguments["resourceId"].ToString()
                        }
                    )
                ));

            // FieldAsync<ListGraphType<LockType>>("locks",
            //     "all locks",
            //     null,
            //     async ctx => await ctx.TryAsyncResolve(async context =>
            //     {
            //         var response = await locationReadClient.GetAllLocationsAsync(new Empty());
            //         return response.Elements;
            //     }));
        }
    }
}