#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Users/UserManager/UserManager.API.csproj", "Users/UserManager/"]
COPY ["Users/UserManager.Core/UserManager.Core.csproj", "Users/UserManager.Core/"]
COPY ["Users/UserManager.Common/UserManager.Common.csproj", "Users/UserManager.Common/"]
COPY ["CommonLib/CommonLib/CommonLib.csproj", "CommonLib/CommonLib/"]
COPY ["Users/User.Interfaces/UserManager.Interfaces.csproj", "Users/User.Interfaces/"]
COPY ["Users/UserManager.MongoPersist/UserManager.MongoPersist.csproj", "Users/UserManager.MongoPersist/"]
RUN dotnet restore "Users/UserManager/UserManager.API.csproj"
COPY . .
WORKDIR "/src/Users/UserManager"
RUN dotnet build "UserManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserManager.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserManager.API.dll"]