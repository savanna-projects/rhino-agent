FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 9000
EXPOSE 9001

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

RUN echo "APT::Get::Assume-Yes \"true\";" > /etc/apt/apt.conf.d/90assumeyes

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
    systemd \
    unzip \
    # python
    python3 \
    python3-pip \
    python3-venv \
    # node.js
    nodejs \
    npm && \
    # cleanup
    apt-get clean && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Set environment variables
ENV BROWSERSTACK_KEY="your_browser_stack_key_here"
ENV ASPNETCORE_URLS=http://+:9000;https://+:9001

WORKDIR /src
COPY ["Rhino.Agent/Rhino.Agent.csproj", "Rhino.Agent/"]
COPY ["Rhino.Controllers/Rhino.Controllers.csproj", "Rhino.Controllers/"]
COPY ["Rhino.Controllers.Domain/Rhino.Controllers.Domain.csproj", "Rhino.Controllers.Domain/"]
COPY ["Rhino.Controllers.Extensions/Rhino.Controllers.Extensions.csproj", "Rhino.Controllers.Extensions/"]
COPY ["Rhino.Controllers.Models/Rhino.Controllers.Models.csproj", "Rhino.Controllers.Models/"]
COPY ["Rhino.Settings/Rhino.Settings.csproj", "Rhino.Settings/"]
RUN dotnet restore "Rhino.Agent/Rhino.Agent.csproj"
COPY . .
WORKDIR "/src/Rhino.Agent"
RUN dotnet build "Rhino.Agent.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rhino.Agent.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

CMD ["/app/Binaries/BrowserStackLocal --force --key $BROWSERSTACK_KEY &"]
ENTRYPOINT ["dotnet", "Rhino.Agent.dll"]
