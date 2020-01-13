#!/bin/bash

dotnet publish ./Microservices/EventSourcing.LocationReadService/EventSourcing.LocationReadService.csproj -o ./Microservices/EventSourcing.LocationReadService/published
docker build -t dkapellusch/eventsourcing.locationreadservice -f ./Microservices/EventSourcing.LocationReadService/Dockerfile ./Microservices/EventSourcing.LocationReadService/
docker push dkapellusch/eventsourcing.locationreadservice
