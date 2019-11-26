using System.Threading.Tasks;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public interface IResolver<TSource, TDestination>
    {
        Task<TDestination> Resolve(TSource source);
    }
}