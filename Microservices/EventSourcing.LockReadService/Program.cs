using System;
using System.IO;
using System.Threading.Tasks;
using EventSourcing.Contracts;
using EventSourcing.Contracts.DataStore;
using EventSourcing.KSQL;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                .AddSingleton<LockStore>()
                .AddSingleton<IReadonlyDataStore<Lock>>(p => p.GetService<LockStore>())
                .AddSingleton<IChangeTracking<Lock>>(p => p.GetService<LockStore>())
                .AddSingleton<LockReadService>()
                .AddGrpc()
            )
            .Configure(builder => builder
                .UseRouting()
                .UseEndpoints(endpointBuilder => endpointBuilder.MapGrpcService<LockReadService>())
            );
    }
}