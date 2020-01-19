using System.Threading.Tasks;

namespace EventSourcing.GraphqlGateway.Graphql
{
    public interface IBidirectionalResolver<TSource, TDestination> : IResolver<TSource, TDestination>
    {
        Task<TSource> Resolve(TDestination source);
    }

    public interface IResolver<TSource, TDestination>
    {
        Task<TDestination> Resolve(TSource source);
    }
}