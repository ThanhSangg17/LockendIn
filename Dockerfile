# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for dependency restoration
COPY ["LockedIn.Api/LockedIn.Api.csproj", "LockedIn.Api/"]
COPY ["LockedIn.BusinessObject/LockedIn.BusinessObject.csproj", "LockedIn.BusinessObject/"]
COPY ["LockedIn.DataAccess/LockedIn.DataAccess.csproj", "LockedIn.DataAccess/"]

# Restore NuGet packages
RUN dotnet restore "LockedIn.Api/LockedIn.Api.csproj"

# Copy the remaining source code
COPY . .

# Build the API project in Release mode
RUN dotnet publish "LockedIn.Api/LockedIn.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Expose port 8080
EXPOSE 8080

# Configure ASP.NET Core URL environment variable
ENV ASPNETCORE_URLS=http://+:8080

# Copy published files from build stage
COPY --from=build /app/publish .

# Define container entry point
ENTRYPOINT ["dotnet", "LockedIn.Api.dll"]
