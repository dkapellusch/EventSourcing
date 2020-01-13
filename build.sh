#!/bin/bash

dotnet clean

bash build-locationread.sh
bash build-locationwrite.sh

bash build-vehicleread.sh
bash build-vehiclewrite.sh

bash build-lockread.sh
bash build-lockwrite.sh

bash build-graphql.sh