using System;
using System.Linq;
using System.Reflection;
using EventSourcing.Contracts;
using EventSourcing.GraphqlGateway.Graphql;
using EventSourcing.GraphqlGateway.Graphql.Types.Vehicle;
using Google.Protobuf.WellKnownTypes;
using GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static EventSourcing.Contracts.LocationRead;
using static EventSourcing.Contracts.LocationWrite;
using static EventSourcing.Contracts.VehicleRead;
using static EventSourcing.Contracts.VehicleWrite;
using Schema = EventSourcing.GraphqlGateway.Graphql.Schema;

namespace EventSourcing.GraphqlGateway
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection AddGraphqlTypes(this IServiceCollection services)
        {
            var currentAssembly = Assembly.GetEntryAssembly();

            foreach (var type in currentAssembly.DefinedTypes.Where(t => typeof(IGraphType).IsAssignableFrom(t))) services.AddSingleton(type.UnderlyingSystemType);

            return services
                .AddSingleton<IDependencyResolver, DependencyResolver>()
                .AddSingleton<IDocumentExecuter, DocumentExecuter>()
                .AddSingleton<IDocumentWriter, DocumentWriter>()
                .AddSingleton<ISchema, Schema>()
                .AddSingleton<Schema>();
        }

        public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration) =>
            services
                .AddSingleton(new VehicleReadClient(new Channel(configuration.GetValue<string>("vehicleRead:host"), ChannelCredentials.Insecure)))
                .AddSingleton(new VehicleWriteClient(new Channel(configuration.GetValue<string>("vehicleWrite:host"), ChannelCredentials.Insecure)))
                .AddSingleton(new LocationReadClient(new Channel(configuration.GetValue<string>("locationRead:host"), ChannelCredentials.Insecure)))
                .AddSingleton(new LocationWriteClient(new Channel(configuration.GetValue<string>("locationWrite:host"), ChannelCredentials.Insecure)))
                .AddSingleton(new LockRead.LockReadClient(new Channel(configuration.GetValue<string>("lockRead:host"), ChannelCredentials.Insecure)))
                .AddSingleton(new LockWrite.LockWriteClient(new Channel(configuration.GetValue<string>("lockWrite:host"), ChannelCredentials.Insecure)));

        public static IServiceCollection AddResolvers(this IServiceCollection services) =>
            services
                .AddSingleton<IResolver<Vehicle, Location>, VehicleLocationResolver>()
                .AddSingleton<IResolver<Vehicle, Lock>, VehicleLockResolver>();

        public static IServiceCollection AddConverters(this IServiceCollection services)
        {
            ValueConverter.Register(
                typeof(Timestamp),
                typeof(DateTime),
                value => ((value as Timestamp)?.ToDateTime()).GetValueOrDefault());
            return services;
        }
    }
}