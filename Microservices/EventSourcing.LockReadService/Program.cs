using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.KSQL;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Google.Protobuf.WellKnownTypes;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;


namespace EventSourcing.LockReadService
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
            .ConfigureKestrel(options => options.ListenAnyIP(7001, o => o.Protocols = HttpProtocols.Http2))
            .ConfigureServices((hostContext, services) => services
                .AddKsql($"http://{Configuration.GetValue<string>("ksql:host")}/query")
                .AddKsqlStore(LockMapper, "Locks", "ActiveLocks_By_ResourceId")
                .AddKafkaConsumer<Lock>(new ConsumerConfig
                {
                    BootstrapServers = Configuration.GetValue<string>("kafka:host"),
                    GroupId = "LockRead",
                    ClientId = Guid.NewGuid().ToString(),
                    AutoOffsetReset = AutoOffsetReset.Earliest
                })
                .AddSingleton<LockReadService>()
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<LockReadService>())
            );


        private static Lock LockMapper(IDictionary<string, dynamic> columns) =>
            new Lock
            {
                LockId = columns.GetValue<Lock, string>(l => l.LockId) ?? string.Empty,
                ResourceId = columns.GetValue<Lock, string>(l => l.ResourceId) ?? string.Empty,
                ResourceType = columns.GetValue<Lock, string>(l => l.ResourceType) ?? string.Empty,
                LockHolderId = columns.GetValue<Lock, string>(l => l.LockHolderId) ?? string.Empty,
                Released = columns.GetValue<Lock, bool>(l => l.Released),
                Expiry = columns.GetValue<Lock, Timestamp>(l => l.Expiry,
                    s => DateTime.Parse(s)
                        .ToUniversalTime()
                        .AddHours(TimeZoneInfo.Local.GetUtcOffset(DateTime.Now).Hours)
                        .ToTimestamp())
            };
    }
}