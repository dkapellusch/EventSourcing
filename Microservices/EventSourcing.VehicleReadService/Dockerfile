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

RUN dotnet restore Microservices/EventSourcing.VehicleReadService/EventSourcing.VehicleReadService.csproj

COPY . .
RUN dotnet publish Microservices/EventSourcing.VehicleReadService/EventSourcing.VehicleReadService.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update && apt-get install -y librocksdb-dev && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5001

ENTRYPOINT ["dotnet", "EventSourcing.VehicleReadService.dll", "--urls", "http://0.0.0.0:5001"]
