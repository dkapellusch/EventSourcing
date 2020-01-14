#!/bin/bash

dotnet publish ./Microservices/EventSourcing.LockWriteService/EventSourcing.LockWriteService.csproj -o ./Microservices/EventSourcing.LockWriteService/published
docker build -t dkapellusch/eventsourcing.lockwriteservice -f ./Microservices/EventSourcing.LockWriteService/Dockerfile ./Microservices/EventSourcing.LockWriteService/
docker push dkapellusch/eventsourcing.lockwriteservice

