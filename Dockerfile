FROM mcr.microsoft.com/dotnet/sdk:9.0-azurelinux3.0 AS build
ARG BUILD_CONFIGURATION=Releaseb
WORKDIR /src
COPY /Journey/ ./Journey/
COPY /shared/ ./shared/
COPY /Journey.Command/ ./Journey.Command/
RUN dotnet restore "./Journey.Command/Journey.Command.csproj"

WORKDIR "/src/."
RUN dotnet build "./Journey.Command/Journey.Command.csproj" 

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Journey.Command/Journey.Command.csproj" -c $BUILD_CONFIGURATION -f net9.0 -o /app/publish

FROM build AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT [ "./Journey.Command"]