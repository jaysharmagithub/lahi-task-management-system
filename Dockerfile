FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/API/TaskManagement.API.csproj", "src/API/"]
COPY ["src/Application/TaskManagement.Application.csproj", "src/Application/"]
COPY ["src/Domain/TaskManagement.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/TaskManagement.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/TaskManagement.API.csproj"
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "TaskManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
# Create the uploads directory and set permissions
RUN mkdir -p /app/uploads && chown -R 100:101 /app/uploads
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD wget -qO- http://localhost:8080/health || exit 1
ENTRYPOINT ["dotnet", "TaskManagement.API.dll"]
