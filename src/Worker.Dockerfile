#┌─[ Container ]────────────────────────────
#│ 
#│ Using Ubuntu aspnet:6.0 as base layer.
#└──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 9000
EXPOSE 9001

#┌─[ Buid ]─────────────────────────────────
#│ 
#│ Using Ubuntu sdk:6.0 as build layer.
#└──────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

#┌─[ Setup ]────────────────────────────────
#│ 
#│ configure apt to not require
#│ confirmation, assume the -y argument
#│ by default.
#└──────────────────────────────────────────
RUN echo "APT::Get::Assume-Yes \"true\";" > /etc/apt/apt.conf.d/90assumeyes

#┌─[ Installation: Tools & SDKs ]───────────
#│ 
#│ Install all the tools and libraries
#│ required by the agent image.
#└──────────────────────────────────────────
# Install tools & SDKs
RUN apt-get update && apt-get install -y --fix-missing --no-install-recommends \
    # tools
    wget \
    curl \
    jq \
    git \
    iputils-ping \
    netcat \
    libssl1.0 \
    vim \
    # python
    python3 \
    python3-pip \
    python3-venv \
    # node.js
    nodejs \
    npm

#┌─[ Deployment ]───────────────────────────
#│ 
#│ Copy Rhino.Api projects into the container.
#└──────────────────────────────────────────
WORKDIR /src
COPY ["Rhino.Worker/Rhino.Worker.csproj", "Rhino.Worker/"]
COPY ["Rhino.Controllers.Domain/Rhino.Controllers.Domain.csproj", "Rhino.Controllers.Domain/"]
COPY ["Rhino.Controllers.Extensions/Rhino.Controllers.Extensions.csproj", "Rhino.Controllers.Extensions/"]
COPY ["Rhino.Controllers.Models/Rhino.Controllers.Models.csproj", "Rhino.Controllers.Models/"]
RUN dotnet restore "Rhino.Worker/Rhino.Worker.csproj"
COPY . .
WORKDIR "/src/Rhino.Worker"
RUN dotnet build "Rhino.Worker.csproj" -c Release -o /app/build

#┌─[ Publish ]──────────────────────────────
#│ 
#│ Publish Rhino.Api services. 
#└──────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "Rhino.Worker.csproj" -c Release -o /app/publish

#┌─[ Setup ]────────────────────────────────
#│ 
#│ Setup Rhino.Api entry point. 
#└──────────────────────────────────────────
FROM base AS final

ENV CONNECTION_TIMEOUT 10
ENV HUB_ADDRESS http://localhost:9000
ENV HUB_API_VERSION 3
ENV MAX_PARALLEL 1

WORKDIR /app
COPY --from=publish /app/publish .
CMD dotnet Rhino.Worker.dll --maxParallel:$MAX_PARALLEL --hubAddress:$HUB_ADDRESS --hubApiVersion:$HUB_API_VERSION --connectionTimeout:$CONNECTION_TIMEOUT

#┌─[ Setup: Arguments & Environment ]───────
#│ 
#│ ASPNETCORE_URLS: The URLs and ports to which the service is listening.
#└──────────────────────────────────────────
ENV ASPNETCORE_URLS=http://+:9000;https://+:9001
