#!/bin/bash

dotnet publish ./Microservices/EventSourcing.LockReadService/EventSourcing.LockReadService.csproj -o ./Microservices/EventSourcing.LockReadService/published
docker build -t dkapellusch/eventsourcing.lockreadservice -f ./Microservices/EventSourcing.LockReadService/Dockerfile ./Microservices/EventSourcing.LockReadService/
docker push dkapellusch/eventsourcing.lockreadservice
