using System;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using FluentValidation;
using Grpc.Core;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.VehicleWriteService
{
    internal class Program
    {
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
            .AddEnvironmentVariables()
            .Build();

        public static async Task Main(string[] args) => await CreateHostBuilder(args).Build().RunAsync();

        private static IWebHostBuilder CreateHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .ConfigureKestrel(options => options.ListenAnyIP(5000, o => o.Protocols = HttpProtocols.Http2))
            .ConfigureServices((hostContext, services) => services
                .AddSingleton<VehicleWriteService>()
                .AddSingleton<IValidator<Vehicle>, VehicleValidator>()
                .AddKafkaProducer<string, Vehicle>(new ProducerConfig
                {
                    BootstrapServers = Configuration.GetValue<string>("kafka:host"),
                    ClientId = Guid.NewGuid().ToString()
                })
                .AddSingleton(new LocationRead.LocationReadClient(new Channel(Configuration.GetValue<string>("locationRead:host"), ChannelCredentials.Insecure)))
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<VehicleWriteService>())
            );
    }
}