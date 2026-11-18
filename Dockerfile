FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/OrderInventory.Api/OrderInventory.Api.csproj", "src/OrderInventory.Api/"]
COPY ["src/OrderInventory.Application/OrderInventory.Application.csproj", "src/OrderInventory.Application/"]
COPY ["src/OrderInventory.Domain/OrderInventory.Domain.csproj", "src/OrderInventory.Domain/"]
COPY ["src/OrderInventory.Infrastructure/OrderInventory.Infrastructure.csproj", "src/OrderInventory.Infrastructure/"]
RUN dotnet restore "src/OrderInventory.Api/OrderInventory.Api.csproj"

COPY . .
RUN dotnet publish "src/OrderInventory.Api/OrderInventory.Api.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM build AS migrate
RUN dotnet tool install --global dotnet-ef --version 10.0.14
ENV PATH="${PATH}:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "src/OrderInventory.Infrastructure/OrderInventory.Infrastructure.csproj", "--startup-project", "src/OrderInventory.Api/OrderInventory.Api.csproj"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "OrderInventory.Api.dll"]
