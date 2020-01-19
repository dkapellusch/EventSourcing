#!/bin/bash

dotnet publish ./EventSourcing.GraphqlGateway/EventSourcing.GraphqlGateway.csproj -o ./EventSourcing.GraphqlGateway/published
docker build -t dkapellusch/eventsourcing.graphql -f ./EventSourcing.GraphqlGateway/Dockerfile ./EventSourcing.GraphqlGateway/
docker push dkapellusch/eventsourcing.graphql
