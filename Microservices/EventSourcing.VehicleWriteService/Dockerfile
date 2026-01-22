FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EventSourcing.sln ./
COPY EventSourcing.Contracts/EventSourcing.Contracts.csproj EventSourcing.Contracts/
COPY EventSourcing.GraphqlGateway/EventSourcing.GraphqlGateway.csproj EventSourcing.GraphqlGateway/
COPY Infrastructure/EventSourcing.Kafka/EventSourcing.Kafka.csproj Infrastructure/EventSourcing.Kafka/
COPY Infrastructure/EventSourcing.KSQL/EventSourcing.KSQL.csproj Infrastructure/EventSourcing.KSQL/
COPY Infrastructure/EventSourcing.RocksDb/EventSourcing.RocksDb.csproj Infrastructure/EventSourcing.RocksDb/
COPY Infrastructure/EventSourcing.Redis/EventSourcing.Redis.csproj Infrastructure/EventSourcing.Redis/
COPY Microservices/EventSourcing.LocationReadService/EventSourcing.LocationReadService.csproj Microservices/EventSourcing.LocationReadService/
COPY Microservices/EventSourcing.LocationWriteService/EventSourcing.LocationWriteService.csproj Microservices/EventSourcing.LocationWriteService/
COPY Microservices/EventSourcing.VehicleReadService/EventSourcing.VehicleReadService.csproj Microservices/EventSourcing.VehicleReadService/
COPY Microservices/EventSourcing.VehicleWriteService/EventSourcing.VehicleWriteService.csproj Microservices/EventSourcing.VehicleWriteService/
COPY Microservices/EventSourcing.LockReadService/EventSourcing.LockReadService.csproj Microservices/EventSourcing.LockReadService/
COPY Microservices/EventSourcing.LockWriteService/EventSourcing.LockWriteService.csproj Microservices/EventSourcing.LockWriteService/

RUN dotnet restore Microservices/EventSourcing.VehicleWriteService/EventSourcing.VehicleWriteService.csproj

COPY . .
RUN dotnet publish Microservices/EventSourcing.VehicleWriteService/EventSourcing.VehicleWriteService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "EventSourcing.VehicleWriteService.dll", "--urls", "http://0.0.0.0:5000"]
