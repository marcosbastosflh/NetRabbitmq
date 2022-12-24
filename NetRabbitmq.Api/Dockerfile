#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["NetRabbitmq.Api/NetRabbitmq.Api.csproj", "NetRabbitmq.Api/"]
RUN dotnet restore "NetRabbitmq.Api/NetRabbitmq.Api.csproj"
COPY . .
WORKDIR "/src/NetRabbitmq.Api"
RUN dotnet build "NetRabbitmq.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NetRabbitmq.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetRabbitmq.Api.dll"]