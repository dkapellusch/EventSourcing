FROM mcr.microsoft.com/dotnet/core/sdk:3.0

RUN mkdir /published
COPY ./published  /published

ENV ASPNETCORE_ENVIRONMENT=Production 

EXPOSE 6000

WORKDIR /published/
ENTRYPOINT ["dotnet", "EventSourcing.LocationWriteService.dll","--environment=Production", "--urls", "http://0.0.0.0:6000"]

