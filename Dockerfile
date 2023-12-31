FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["YandexMarketService.API/YandexMarketService.API.csproj", "YandexMarketService.API/"]
COPY ["YandexMarketService.BLL/YandexMarketService.BLL.csproj", "YandexMarketService.BLL/"]
COPY ["YandexMarketService.DAL/YandexMarketService.DAL.csproj", "YandexMarketService.DAL/"]
RUN dotnet restore "YandexMarketService.API/YandexMarketService.API.csproj"
COPY . .
WORKDIR "/src/YandexMarketService.API"
RUN dotnet build "YandexMarketService.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "YandexMarketService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

run apt update
run apt install curl -y
run apt-get install -y ca-certificates curl gnupg
run mkdir -p /etc/apt/keyrings
run curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg
run echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_20.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list
run apt update
run apt install nodejs -y
run npx playwright install firefox
run npx playwright install-deps firefox
# remove this line
run mv /root/.cache/ms-playwright/firefox-1429 /root/.cache/ms-playwright/firefox-1425


COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "YandexMarketService.API.dll"]