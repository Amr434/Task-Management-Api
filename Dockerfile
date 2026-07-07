FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["Task-Management.slnx", "./"]
COPY ["Task-Management.Api/Task-Management.Api.csproj", "Task-Management.Api/"]
COPY ["Task-Management.Application/Task-Management.Application.csproj", "Task-Management.Application/"]
COPY ["Task-Management.Domain/Task-Management.Domain.csproj", "Task-Management.Domain/"]
COPY ["Task-Management.Infrastructure/Task-Management.Infrastructure.csproj", "Task-Management.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "Task-Management.Api/Task-Management.Api.csproj"

# Copy the rest of the code
COPY . .
WORKDIR "/src/Task-Management.Api"
RUN dotnet build "Task-Management.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Task-Management.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Task-Management.Api.dll"]
