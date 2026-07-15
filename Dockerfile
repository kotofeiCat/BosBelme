FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["BosBelme/BosBelme.csproj", "BosBelme/"]
COPY ["BosBelme.Service/BosBelme.Service.csproj", "BosBelme.Service/"]
COPY ["BosBelme.Data/BosBelme.Data.csproj", "BosBelme.Data/"]

RUN dotnet restore "BosBelme/BosBelme.csproj"

# 3. Копируем абсолютно весь исходный код решения
COPY . .

WORKDIR "/src/BosBelme"
RUN dotnet build "BosBelme.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BosBelme.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BosBelme.dll"]