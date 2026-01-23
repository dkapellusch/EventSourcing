using EventSourcing.GraphqlGateway;
using EventSourcing.GraphqlGateway.Graphql;
using EventSourcing.GraphqlGateway.Graphql.Types;
using EventSourcing.GraphqlGateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGrpcClients(builder.Configuration)
    .AddHostedService<GrpcSubscriptionBridge>();

builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddSubscriptionType<Subscription>()
    .AddTypeExtension<VehicleTypeExtension>()
    .AddInMemorySubscriptions()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = true)
    .AllowIntrospection(true);

var app = builder.Build();

app.UseWebSockets();
app.MapGraphQL("/api/graphql");

app.Run();
