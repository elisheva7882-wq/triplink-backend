FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 10000
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TripLink.API/TripLink.API.csproj", "TripLink.API/"]
COPY ["TripLink.Core/TripLink.Core.csproj", "TripLink.Core/"]
COPY ["TripLink.Data/TripLink.Data.csproj", "TripLink.Data/"]
COPY ["TripLink.Service/TripLink.Service.csproj", "TripLink.Service/"]

RUN dotnet restore "TripLink.API/TripLink.API.csproj"

COPY . .

WORKDIR "/src/TripLink.API"
RUN dotnet publish "TripLink.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TripLink.API.dll"]