using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.KSQL;
using EventSourcing.RocksDb.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.VehicleReadService
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
            .ConfigureKestrel(options => options.ListenAnyIP(5001, o => o.Protocols = HttpProtocols.Http2))
            .ConfigureServices((hostContext, services) => services
                .AddSingleton(new KsqlClient(new HttpClient {BaseAddress = new Uri($"http://{Configuration.GetValue<string>("ksql:host")}/query")}))
                .AddSingleton<KafkaKsqlQueryExecutor>()
                .AddSingleton<VehicleKsqlTable>()
                .AddSingleton<KsqlVehicleReadService>()
                .AddSingleton<VehicleReadService>()
                .AddKafkaConsumer<Vehicle>(new ConsumerConfig
                {
                    BootstrapServers = Configuration.GetValue<string>("kafka:host"),
                    GroupId = "VehicleRead",
                    ClientId = Guid.NewGuid().ToString(),
                    AutoOffsetReset = AutoOffsetReset.Earliest
                })
                .AddRocksDb(Configuration.GetValue<string>("rocks:path"))
                .AddSingleton(typeof(KafkaBackedDb<>))
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<VehicleReadService>())
            );
    }
}