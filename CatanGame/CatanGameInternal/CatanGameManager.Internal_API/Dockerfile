FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CommonLib/CommonLib/", "CommonLib/CommonLib/"]
COPY ["CatanGame/CatanGameManager.Common/", "CatanGame/CatanGameManager.Common/"]
COPY ["CatanGame/CatanGameInternal/", "CatanGame/CatanGameInternal/"]
RUN dotnet restore "CatanGame/CatanGameInternal/CatanGameManager.API/CatanGameManager.Internal_API.csproj"

FROM build AS publish
WORKDIR "/src/CatanGame/CatanGameInternal/CatanGameManager.API"
RUN dotnet publish "CatanGameManager.Internal_API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .


ENTRYPOINT ["dotnet", "CatanGameManager.Internal_API.dll"]