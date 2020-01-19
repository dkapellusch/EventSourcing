using EventSourcing.GraphqlGateway.Graphql.Types.Location;
using EventSourcing.GraphqlGateway.Graphql.Types.Lock;
using EventSourcing.GraphqlGateway.Graphql.Types.Vehicle;
using GraphQL;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public class RootMutation : ObjectGraphType
    {
        private readonly IDependencyResolver _resolver;

        public RootMutation(IDependencyResolver resolver)
        {
            _resolver = resolver;
            AddMutations<VehicleMutations>();
            AddMutations<LocationMutations>();
            AddMutations<LockMutations>();
        }

        private void AddMutations<T>() where T : IComplexGraphType
        {
            var subQuery = _resolver.Resolve<T>();
            foreach (var field in subQuery.Fields)
                Field(field.Type, field.Name, field.Description, field.Arguments, ctx => field.Resolver.Resolve(ctx as ResolveFieldContext));
        }
    }
}