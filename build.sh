#!/bin/bash

dotnet clean

dotnet publish ./Microservices/EventSourcing.LocationReadService/EventSourcing.LocationReadService.csproj -o ./Microservices/EventSourcing.LocationReadService/published
docker build -t dkapellusch/eventsourcing.locationreadservice -f ./Microservices/EventSourcing.LocationReadService/Dockerfile ./Microservices/EventSourcing.LocationReadService/

dotnet publish ./Microservices/EventSourcing.LocationWriteService/EventSourcing.LocationWriteService.csproj -o ./Microservices/EventSourcing.LocationWriteService/published
docker build -t dkapellusch/eventsourcing.locationwriteservice -f ./Microservices/EventSourcing.LocationWriteService/Dockerfile ./Microservices/EventSourcing.LocationWriteService/

dotnet publish ./Microservices/EventSourcing.VehicleReadService/EventSourcing.VehicleReadService.csproj -o ./Microservices/EventSourcing.VehicleReadService/published
docker build -t dkapellusch/eventsourcing.vehiclereadservice -f ./Microservices/EventSourcing.VehicleReadService/Dockerfile ./Microservices/EventSourcing.VehicleReadService/

dotnet publish ./Microservices/EventSourcing.VehicleWriteService/EventSourcing.VehicleWriteService.csproj -o ./Microservices/EventSourcing.VehicleWriteService/published
docker build -t dkapellusch/eventsourcing.vehiclewriteservice -f ./Microservices/EventSourcing.VehicleWriteService/Dockerfile ./Microservices/EventSourcing.VehicleWriteService/

dotnet publish ./Microservices/EventSourcing.LockReadService/EventSourcing.LockReadService.csproj -o ./Microservices/EventSourcing.LockReadService/published
docker build -t dkapellusch/eventsourcing.lockreadservice -f ./Microservices/EventSourcing.LockReadService/Dockerfile ./Microservices/EventSourcing.LockReadService/

dotnet publish ./Microservices/EventSourcing.LockWriteService/EventSourcing.LockWriteService.csproj -o ./Microservices/EventSourcing.LockWriteService/published
docker build -t dkapellusch/eventsourcing.lockwriteservice -f ./Microservices/EventSourcing.LockWriteService/Dockerfile ./Microservices/EventSourcing.LockWriteService/

dotnet publish ./EventSourcing.GraphqlGateway/EventSourcing.GraphqlGateway.csproj -o ./EventSourcing.GraphqlGateway/published
docker build -t dkapellusch/eventsourcing.graphql -f ./EventSourcing.GraphqlGateway/Dockerfile ./EventSourcing.GraphqlGateway/

docker push dkapellusch/eventsourcing.locationreadservice
docker push dkapellusch/eventsourcing.locationwriteservice

docker push dkapellusch/eventsourcing.vehiclereadservice
docker push dkapellusch/eventsourcing.vehiclewriteservice

docker push dkapellusch/eventsourcing.lockreadservice
docker push dkapellusch/eventsourcing.lockwriteservice

docker push dkapellusch/eventsourcing.graphql
