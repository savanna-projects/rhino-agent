# Setup: Base
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 9000
EXPOSE 9001

# Build
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

# Install Utilities
#RUN apt-get update && apt-get install -y wget
#RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
#RUN dpkg -i packages-microsoft-prod.deb
#RUN apt-get update && apt-get install -y \
#    curl \
#    apt-transport-https \
#    dotnet-sdk-5.0 \
#    gss-ntlmssp \
#    vim \
#    jq \
#    sudo \
#    openssh-server sudo -y

# deploy
WORKDIR /src
COPY ["Rhino.Agent/Rhino.Agent.csproj", "Rhino.Agent/"]
COPY ["Rhino.Controllers/Rhino.Controllers.csproj", "Rhino.Controllers/"]
COPY ["Rhino.Controllers.Domain/Rhino.Controllers.Domain.csproj", "Rhino.Controllers.Domain/"]
COPY ["Rhino.Controllers.Extensions/Rhino.Controllers.Extensions.csproj", "Rhino.Controllers.Extensions/"]
COPY ["Rhino.Controllers.Models/Rhino.Controllers.Models.csproj", "Rhino.Controllers.Models/"]
RUN dotnet restore --disable-parallel "Rhino.Agent/Rhino.Agent.csproj"
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

# Environment
ENV ASPNETCORE_URLS=http://+:9000;https://+:9001
