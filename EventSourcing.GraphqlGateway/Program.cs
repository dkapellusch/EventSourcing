using System;
using System.IO;
using System.Threading.Tasks;
using EventSourcing.GraphqlGateway.Graphql;
using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventSourcing.GraphqlGateway
{
    internal class Program
    {
        private static readonly IConfiguration Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
            .AddEnvironmentVariables()
            .Build();

        private static Task Main(string[] args) => CreateWebHostBuilder(args).Build().RunAsync();

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost.CreateDefaultBuilder(args)
            .UseConfiguration(Configuration)
            .ConfigureServices(services => services
                .AddGrpcClients(Configuration)
                .AddLogging()
                .AddResolvers()
                .AddConverters()
                .AddGraphqlTypes()
                .AddHttpContextAccessor()
                .AddGraphQL()
                .AddWebSockets()
                .AddDataLoader()
            )
            .Configure((context, builder) => builder
                .UseWebSockets()
                .UseGraphQLWebSockets<Schema>("/api/graphql")
                .UseMiddleware<GraphQLMiddleware>()
                .UseGraphQLPlayground(new GraphQLPlaygroundOptions {Path = "/api/graphql/playground", GraphQLEndPoint = "/api/graphql"})
                .UseGraphQLVoyager(new GraphQLVoyagerOptions {Path = "/api/graphql/voyager", GraphQLEndPoint = "/api/graphql"})
                .UseGraphiQLServer(new GraphiQLOptions {GraphiQLPath = "/api/graphql/graphiql", GraphQLEndPoint = "/api/graphql"})
            )
            .UseKestrel()
            .ConfigureKestrel(k => k.AllowSynchronousIO = true);
    }
}