#!/bin/bash

dotnet publish ./Microservices/EventSourcing.VehicleWriteService/EventSourcing.VehicleWriteService.csproj -o ./Microservices/EventSourcing.VehicleWriteService/published
docker build -t dkapellusch/eventsourcing.vehiclewriteservice -f ./Microservices/EventSourcing.VehicleWriteService/Dockerfile ./Microservices/EventSourcing.VehicleWriteService/
docker push dkapellusch/eventsourcing.vehiclewriteservice
