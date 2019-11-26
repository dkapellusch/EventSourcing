using EventSourcing.GraphqlGateway.Graphql.Vehicle;
using GraphQL;
using GraphQL.Types;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public class RootSubscription : ObjectGraphType
    {
        private readonly IDependencyResolver _resolver;

        public RootSubscription(IDependencyResolver resolver)
        {
            _resolver = resolver;
            AddSubscriptions<VehicleSubscription>();
        }

        private void AddSubscriptions<T>() where T : IComplexGraphType
        {
            var subQuery = _resolver.Resolve<T>();
            foreach (var field in subQuery.Fields)
            {
                AddField(field);
            }
        }
    }
}