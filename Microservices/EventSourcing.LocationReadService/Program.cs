using System;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.RocksDb.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.LocationReadService
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
            .ConfigureKestrel(options => options.ListenAnyIP(6001, o => o.Protocols = HttpProtocols.Http2))
            .ConfigureServices((hostContext, services) => services
                .AddSingleton<LocationReadService>()
                .AddKafkaConsumer<Location>(new ConsumerConfig
                {
                    BootstrapServers = Configuration.GetValue<string>("kafka:host"),
                    GroupId = Guid.NewGuid().ToString(),
                    ClientId = Guid.NewGuid().ToString(),
                    AutoOffsetReset = AutoOffsetReset.Earliest
                })
                .AddRocksDb(Configuration.GetValue<string>("rocks:path"))
                .AddSingleton(typeof(KafkaBackedDb<>))
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<LocationReadService>())
            );
    }
}