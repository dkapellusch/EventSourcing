FROM mcr.microsoft.com/dotnet/core/sdk:3.0

RUN apt-get update
RUN  apt-get install -y libgflags-dev libsnappy-dev zlib1g-dev libbz2-dev libzstd-dev librocksdb-dev

RUN mkdir /published
COPY ./published  /published

ENV ASPNETCORE_ENVIRONMENT=Production 

EXPOSE 7001

WORKDIR /published/
ENTRYPOINT ["dotnet", "EventSourcing.LockReadService.dll","--environment=Production", "--urls", "http://0.0.0.0:7001"]

