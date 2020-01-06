using System;
using GraphQL;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.GraphqlGateway
{
    public class DependencyResolver : IDependencyResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public DependencyResolver(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public T Resolve<T>() => _serviceProvider.GetService<T>();

        public object Resolve(Type type) => _serviceProvider.GetService(type);
    }
}