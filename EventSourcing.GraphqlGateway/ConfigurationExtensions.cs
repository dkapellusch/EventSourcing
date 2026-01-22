using EventSourcing.Contracts;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using static EventSourcing.Contracts.LocationRead;
using static EventSourcing.Contracts.LocationWrite;
using static EventSourcing.Contracts.VehicleRead;
using static EventSourcing.Contracts.VehicleWrite;

namespace EventSourcing.GraphqlGateway;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddGrpcClients(this IServiceCollection services, IConfiguration configuration) =>
        services
            .AddSingleton(new VehicleReadClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("vehicleRead:host")}")))
            .AddSingleton(new VehicleWriteClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("vehicleWrite:host")}")))
            .AddSingleton(new LocationReadClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("locationRead:host")}")))
            .AddSingleton(new LocationWriteClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("locationWrite:host")}")))
            .AddSingleton(new LockRead.LockReadClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("lockRead:host")}")))
            .AddSingleton(new LockWrite.LockWriteClient(GrpcChannel.ForAddress($"http://{configuration.GetValue<string>("lockWrite:host")}")));
}
