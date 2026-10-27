# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src

COPY Marsa.sln Directory.Build.props Directory.Packages.props NuGet.config global.json ./
COPY src/Marsa.Contracts/Marsa.Contracts.csproj src/Marsa.Contracts/packages.lock.json src/Marsa.Contracts/
COPY src/Marsa.Domain/Marsa.Domain.csproj src/Marsa.Domain/packages.lock.json src/Marsa.Domain/
COPY src/Marsa.Application/Marsa.Application.csproj src/Marsa.Application/packages.lock.json src/Marsa.Application/
COPY src/Marsa.Infrastructure/Marsa.Infrastructure.csproj src/Marsa.Infrastructure/packages.lock.json src/Marsa.Infrastructure/
COPY src/Marsa.Server/Marsa.Server.csproj src/Marsa.Server/packages.lock.json src/Marsa.Server/
COPY src/Marsa.Cli/Marsa.Cli.csproj src/Marsa.Cli/packages.lock.json src/Marsa.Cli/
COPY tests/Marsa.Domain.Tests/Marsa.Domain.Tests.csproj tests/Marsa.Domain.Tests/packages.lock.json tests/Marsa.Domain.Tests/
COPY tests/Marsa.Application.Tests/Marsa.Application.Tests.csproj tests/Marsa.Application.Tests/packages.lock.json tests/Marsa.Application.Tests/
COPY tests/Marsa.Infrastructure.Tests/Marsa.Infrastructure.Tests.csproj tests/Marsa.Infrastructure.Tests/packages.lock.json tests/Marsa.Infrastructure.Tests/
COPY tests/Marsa.Server.Tests/Marsa.Server.Tests.csproj tests/Marsa.Server.Tests/packages.lock.json tests/Marsa.Server.Tests/

RUN dotnet restore Marsa.sln --locked-mode

COPY src/ src/

WORKDIR /src/src/Marsa.Server
RUN dotnet publish -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime

# Create non-root marsa user (UID/GID 10001) and writable state directories
RUN groupadd -g 10001 marsa && \
    useradd -u 10001 -g marsa -d /var/lib/marsa -s /usr/sbin/nologin marsa && \
    mkdir -p /var/lib/marsa /tmp /app && \
    chown -R 10001:10001 /var/lib/marsa /tmp /app

WORKDIR /app
COPY --from=build --chown=10001:10001 /app/publish .

ENV ASPNETCORE_HTTP_PORTS=5080 \
    DOTNET_EnableDiagnostics=0 \
    HOME=/var/lib/marsa

USER 10001:10001

EXPOSE 5080

ENTRYPOINT ["dotnet", "Marsa.Server.dll"]
