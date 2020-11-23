# Setup: Base
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 9000
EXPOSE 9001

# Build
FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["Rhino.Agent/Rhino.Agent.csproj", "Rhino.Agent/"]
RUN dotnet restore "Rhino.Agent/Rhino.Agent.csproj"
COPY . .
WORKDIR "/src/Rhino.Agent"
RUN dotnet build "Rhino.Agent.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Rhino.Agent.csproj" -c Release -o /app/publish

# Setup: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rhino.Agent.dll"]

# Install Utilities
RUN apt-get update && \
    apt-get install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y \
    curl \
    apt-transport-https \
    dotnet-sdk-3.1 \
    vim \
    jq     

# Environment
ENV ASPNETCORE_URLS=http://+:9000;https://+:9001