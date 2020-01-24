using System;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using EventSourcing.Contracts;
using EventSourcing.Kafka;
using EventSourcing.Redis;
using EventSourcing.RocksDb.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.NotificationWrite
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
            var notificationService = host.Services.GetService<NotificationWriteService>();
            await notificationService.StartExistingTimers();
            await host.RunAsync();
        }

        private static IWebHostBuilder CreateHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .ConfigureKestrel(options =>
                options.ListenAnyIP(Configuration.GetValue<int>("port"), o => o.Protocols = HttpProtocols.Http2)
            )
            .ConfigureServices((hostContext, services) => services
                .AddSingleton<NotificationWriteService>()
                .AddKafkaProducer<string, Notification>(new ProducerConfig
                {
                    BootstrapServers = Configuration.GetValue<string>("kafka:host"),
                    ClientId = Guid.NewGuid().ToString()
                })
                .AddRocksDb("./notifications.db")
                .AddRedisDataStore(Configuration.GetValue<string>("redis:host"))
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<NotificationWriteService>())
            );
    }
}