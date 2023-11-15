#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["RiseDiary.WebAPI/RiseDiary.WebAPI.csproj", "RiseDiary.WebAPI/"]
COPY ["RiseDiary.Front/RiseDiary.Front.csproj", "RiseDiary.Front/"]
COPY ["RiseDiary.Shared/RiseDiary.Shared.csproj", "RiseDiary.Shared/"]
RUN dotnet restore "./RiseDiary.WebAPI/./RiseDiary.WebAPI.csproj"
COPY . .
WORKDIR "/src/RiseDiary.WebAPI"
RUN dotnet build "./RiseDiary.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./RiseDiary.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RiseDiary.WebAPI.dll"]