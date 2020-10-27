# Setup: Base
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build
FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
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
RUN apt-get update && apt-get install -y wget
RUN wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update && apt-get install -y \
    apt-transport-https \
    dotnet-sdk-3.1 \
    vim \
    jq
RUN dotnet dev-certs https

# Setup: Rhino Development Certificate
RUN printf '%s\n' \
        '[ req ]' \
        'default_bits       = 2048' \
        'default_keyfile    = https.key' \
        'distinguished_name = req_distinguished_name' \
        'req_extensions     = req_ext' \
        'x509_extensions    = v3_ca' \
        \
        '[ req_distinguished_name ]' \
        'commonName                  = Rhino API HTTPS development certificate' \
        \
        '[ req_ext ]' \
        'subjectAltName = @alt_names' \
        \
        '[ v3_ca ]' \
        'subjectAltName = @alt_names' \
        'basicConstraints = critical, CA:false' \
        'keyUsage = keyCertSign, cRLSign, digitalSignature, keyEncipherment' \
        \
        '[ alt_names ]' \
        'DNS.1   = localhost' \
        'DNS.2   = 127.0.0.1' > https.config && \
    openssl req -x509 -subj '/commonName=Rhino API HTTPS development certificate' -nodes -days 365 -newkey rsa:2048 -keyout https.key -out https.crt -config https.config && \
    openssl pkcs12 -export -out https.pfx -inkey https.key -in https.crt -passin pass: -passout pass: && \
    openssl verify -CAfile https.crt https.crt && \
    cp https.crt /usr/local/share/ca-certificates/ && \
    update-ca-certificates && \
    ls /etc/ssl/certs/https.pem && \
    openssl verify https.crt && \
    cat appsettings.json | jq  '. + { "Kestrel": { "Certificates": { "Default": { "Path": "/app/https.pfx", "Password": "" } } } }'


# Environment
ENV ASPNETCORE_URLS=http://+:80;https://+:443