using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.Extensions;
using EventSourcing.KSQL;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var streamCreator = host.Services.GetService<KsqlStreamCreator>();
            await streamCreator.EnsureStreamExists(new KsqlQuery
                {
                    Ksql = "Create stream Locks (ResourceId string, LockHolderId string, LockId string, ResourceType string, Expiry string) WITH (KAFKA_TOPIC = 'Locks', VALUE_FORMAT = 'json');",
                    StreamProperties = {KsqlQuery.OffsetEarliest}
                },
                new KsqlQuery
                {
                    Ksql =
                        "create stream EnrichedLocks as select ResourceId,LockHolderId,LockId,ResourceType,Expiry, STRINGTOTimeStamp(Expiry,'yyyy-MM-dd''T''HH:mm:ss.SSSSSS''Z''','UTC') <= UNIX_TIMESTAMP() as Inactive from Locks;",
                    StreamProperties = {KsqlQuery.OffsetEarliest}
                },
                new KsqlQuery
                {
                    Ksql = "create stream ActiveLocks as select * from EnrichedLocks where Inactive = false;",
                    StreamProperties = {KsqlQuery.OffsetEarliest}
                },
                new KsqlQuery
                {
                    Ksql =
                        "create table ActiveLocks_By_ResourceId as select RESOURCEID, count(RESOURCEID) as lockedCount, collect_set(LOCKHOLDERID) as LOCKHOLDERID, collect_set(RESOURCETYPE) as RESOURCETYPE , collect_set(lockId) as lockId, collect_set(expiry) as expiry, collect_set(Inactive) as Inactive from ActiveLocks group by RESOURCEID;",
                    StreamProperties = {KsqlQuery.OffsetEarliest}
                }
            );
            await host.RunAsync();
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .ConfigureKestrel(options => options.ListenAnyIP(Configuration.GetValue<int>("port"), o => o.Protocols = HttpProtocols.Http2))
            .ConfigureServices((hostContext, services) => services
                .AddKsql($"http://{Configuration.GetValue<string>("ksql:host")}")
                .AddKsqlStore(
                    LockMapper,
                    new KsqlQuery {Ksql = "Select * from  Locks emit changes;"},
                    new KsqlQuery {Ksql = "select * from ActiveLocks_By_ResourceId where rowkey = {0};"}
                )
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
                Expiry = columns.GetValue<Lock, Timestamp>(l => l.Expiry, s => s.ParseTimeStamp())
            };
    }
}