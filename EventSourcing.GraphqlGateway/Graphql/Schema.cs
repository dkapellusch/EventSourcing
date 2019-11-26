using GraphQL;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public class Schema : GraphQL.Types.Schema
    {
        public Schema(IDependencyResolver resolver)
        {
            DependencyResolver = resolver;
            Query = resolver.Resolve<RootQuery>();
            Mutation = resolver.Resolve<RootMutation>();
            Subscription = resolver.Resolve<RootSubscription>();
        }
    }
}