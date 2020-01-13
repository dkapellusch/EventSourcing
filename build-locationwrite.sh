#!/bin/bash

dotnet publish ./Microservices/EventSourcing.LocationWriteService/EventSourcing.LocationWriteService.csproj -o ./Microservices/EventSourcing.LocationWriteService/published
docker build -t dkapellusch/eventsourcing.locationwriteservice -f ./Microservices/EventSourcing.LocationWriteService/Dockerfile ./Microservices/EventSourcing.LocationWriteService/
docker push dkapellusch/eventsourcing.locationwriteservice
