FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY NihonCheck.csproj ./
RUN dotnet restore ./NihonCheck.csproj
COPY . .
RUN dotnet publish ./NihonCheck.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=10000
ENV PRISMA_DATA_DIR=/data
EXPOSE 10000
RUN mkdir -p /data && chown -R $APP_UID:$APP_UID /data
VOLUME ["/data"]
COPY --from=build /app/publish .
USER $APP_UID
ENTRYPOINT ["dotnet", "NihonCheck.dll"]
