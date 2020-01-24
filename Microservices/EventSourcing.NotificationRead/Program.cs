using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.KSQL;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.NotificationRead
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
            .ConfigureKestrel(options =>
                options.ListenAnyIP(Configuration.GetValue<int>("port"), o => o.Protocols = HttpProtocols.Http2)
            )
            .ConfigureServices((hostContext, services) => services
                .AddSingleton<NotificationReadService>()
                .AddKsql($"http://{Configuration.GetValue<string>("ksql:host")}")
                .AddKsqlStore(
                    NotificationMapper,
                    new KsqlQuery {Ksql = "Select * from  ReadyNotifications emit changes;"},
                    new KsqlQuery {Ksql = "select * from PendingNotifications_By_NotificationId where rowkey = '{0}';"}
                )
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<NotificationReadService>())
            );

        private static Notification NotificationMapper(IDictionary<string, dynamic> columns) =>
            new Notification
            {
                NotificationId = columns.GetValue<Notification, string>(n => n.NotificationId) ?? string.Empty,
                Category = columns.GetValue<Notification, string>(n => n.Category) ?? string.Empty,
                Data = columns.GetValue<Notification, string>(n => n.Data) ?? string.Empty,
                NotificationTime = columns.GetValue<Notification, long>(l => l.NotificationTime)
            };
    }
}