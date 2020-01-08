using EventSourcing.GraphqlGateway.Graphql.Location;
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
            AddSubscriptions<LocationSubscription>();
        }

        private void AddSubscriptions<T>() where T : IComplexGraphType
        {
            var subQuery = _resolver.Resolve<T>();
            foreach (var field in subQuery.Fields) AddField(field);
        }
    }
}