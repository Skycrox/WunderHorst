FROM mcr.microsoft.com/dotnet/sdk:10.0-noble-aot AS build
WORKDIR /src

COPY Directory.Build.props ./
COPY src/backend/WunderHorst.Server/WunderHorst.Server.csproj ./src/backend/WunderHorst.Server/
RUN dotnet restore src/backend/WunderHorst.Server/WunderHorst.Server.csproj

COPY src/backend/WunderHorst.Server/ ./src/backend/WunderHorst.Server/
RUN dotnet publish src/backend/WunderHorst.Server/WunderHorst.Server.csproj \
    -c Release \
    -o /app/publish \
    -p:OpenApiGenerateDocumentsOnBuild=false && \
    mkdir -p /tmp/data

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled-extra AS runtime
WORKDIR /app

COPY --from=build /app/publish/WunderHorst.Server ./WunderHorst.Server
COPY --from=build --chown=1654:1654 /tmp/data /data

ENV Database__ConnectionString="Data Source=/data/wunderhorst.db"

VOLUME ["/data"]
EXPOSE 18420 18443

ENTRYPOINT ["./WunderHorst.Server"]
