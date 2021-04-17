#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["CatanGame/CatanGameManager.API/CatanGameManager.API.csproj", "CatanGame/CatanGameManager.API/"]
COPY ["CatanGame/CatanGame.Interfaces/CatanGameManager.Interfaces.csproj", "CatanGame/CatanGame.Interfaces/"]
COPY ["CatanGame/CatanGameManager.Common/CatanGameManager.Common.csproj", "CatanGame/CatanGameManager.Common/"]
COPY ["CommonLib/CommonLib/CommonLib.csproj", "CommonLib/CommonLib/"]
COPY ["CatanGame/CatanGameManager.Core/CatanGameManager.Core.csproj", "CatanGame/CatanGameManager.Core/"]
COPY ["CatanGame/CatanGameManger.Persistence.Mongo/CatanGameManger.Persistence.Mongo.csproj", "CatanGame/CatanGameManger.Persistence.Mongo/"]
RUN dotnet restore "CatanGame/CatanGameManager.API/CatanGameManager.API.csproj"
COPY . .
WORKDIR "/src/CatanGame/CatanGameManager.API"
RUN dotnet build "CatanGameManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CatanGameManager.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatanGameManager.API.dll"]