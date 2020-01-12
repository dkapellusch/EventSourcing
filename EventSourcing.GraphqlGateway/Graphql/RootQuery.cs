using EventSourcing.GraphqlGateway.Graphql.Types.Location;
using EventSourcing.GraphqlGateway.Graphql.Types.Vehicle;
using GraphQL;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public class RootQuery : ObjectGraphType
    {
        private readonly IDependencyResolver _resolver;

        public RootQuery(IDependencyResolver resolver)
        {
            _resolver = resolver;
            AddQueries<VehicleQueries>();
            AddQueries<LocationQueries>();
        }

        private void AddQueries<T>() where T : IComplexGraphType
        {
            var subQuery = _resolver.Resolve<T>();
            foreach (var field in subQuery.Fields)
                Field(field.Type, field.Name, field.Description, field.Arguments, ctx => field.Resolver.Resolve(ctx as ResolveFieldContext));
        }
    }
}