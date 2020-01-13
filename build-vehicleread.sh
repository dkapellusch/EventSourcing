#!/bin/bash

dotnet publish ./Microservices/EventSourcing.VehicleReadService/EventSourcing.VehicleReadService.csproj -o ./Microservices/EventSourcing.VehicleReadService/published
docker build -t dkapellusch/eventsourcing.vehiclereadservice -f ./Microservices/EventSourcing.VehicleReadService/Dockerfile ./Microservices/EventSourcing.VehicleReadService/
docker push dkapellusch/eventsourcing.vehiclereadservice

