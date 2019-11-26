FROM mcr.microsoft.com/dotnet/core/sdk:3.0

RUN mkdir /published
COPY ./published  /published

ENV ASPNETCORE_ENVIRONMENT=Production 

EXPOSE 5000

WORKDIR /published/
ENTRYPOINT ["dotnet", "EventSourcing.VehicleWriteService.dll","--environment=Production", "--urls", "http://0.0.0.0:5000"]

