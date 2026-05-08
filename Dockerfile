FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS base
USER app
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["Taskeen.API/Taskeen.API.csproj", "Taskeen.API/"]
COPY ["Taskeen.Application/Taskeen.Application.csproj", "Taskeen.Application/"]
COPY ["Taskeen.Domain/Taskeen.Domain.csproj", "Taskeen.Domain/"]
COPY ["Taskeen.Infrastructure/Taskeen.Infrastructure.csproj", "Taskeen.Infrastructure/"]
RUN dotnet restore "Taskeen.API/Taskeen.API.csproj"
COPY . .
WORKDIR "/src/Taskeen.API"
RUN dotnet build "Taskeen.API.csproj" -c Release -o /app/build
FROM build AS publish
RUN dotnet publish "Taskeen.API.csproj" -c Release -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=publish --chown=app:app /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Taskeen.API.dll"]
