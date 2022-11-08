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
#│ requierd by the agent image.
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
COPY ["Rhino.Agent/Rhino.Agent.csproj", "Rhino.Agent/"]
COPY ["Rhino.Controllers/Rhino.Controllers.csproj", "Rhino.Controllers/"]
COPY ["Rhino.Controllers.Domain/Rhino.Controllers.Domain.csproj", "Rhino.Controllers.Domain/"]
COPY ["Rhino.Controllers.Extensions/Rhino.Controllers.Extensions.csproj", "Rhino.Controllers.Extensions/"]
COPY ["Rhino.Controllers.Models/Rhino.Controllers.Models.csproj", "Rhino.Controllers.Models/"]
RUN dotnet restore "Rhino.Agent/Rhino.Agent.csproj"
COPY . .
WORKDIR "/src/Rhino.Agent"
RUN dotnet build "Rhino.Agent.csproj" -c Release -o /app/build

#┌─[ Publish ]──────────────────────────────
#│ 
#│ Publish Rhino.Api services. 
#└──────────────────────────────────────────
FROM build AS publish
RUN dotnet publish "Rhino.Agent.csproj" -c Release -o /app/publish

#┌─[ Setup ]────────────────────────────────
#│ 
#│ Setup Rhino.Api entry point. 
#└──────────────────────────────────────────
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rhino.Agent.dll"] 

#┌─[ Setup: Arguments & Environement ]──────
#│ 
#│ ASPNETCORE_URLS: The URLs and ports to which the service is listening.
#└──────────────────────────────────────────
ENV ASPNETCORE_URLS=http://+:9000;https://+:9001
