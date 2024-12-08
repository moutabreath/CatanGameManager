#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["CommonLib/CommonLib/", "CommonLib/CommonLib/"]
COPY ["CatanGame/CatanGameManager.Common/", "CatanGame/CatanGameManager.Common/"]
COPY ["CatanGame/CatanGameExternal/", "CatanGame/CatanGameExternal/"]
RUN dotnet restore "CatanGame/CatanGameExternal/CatanGameManager.External_API/CatanGameManager.CatanGameExternal_API.csproj"

FROM build AS publish
WORKDIR "/src/CatanGame/CatanGameExternal/CatanGameManager.External_API"
RUN dotnet publish "CatanGameManager.External_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CatanGameManager.External_API.dll"]
