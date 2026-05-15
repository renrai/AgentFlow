FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AgentFlow.slnx", "./"]
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
COPY ["global.json", "./"]
COPY ["src/AgentFlow.Api/AgentFlow.Api.csproj", "src/AgentFlow.Api/"]
COPY ["src/AgentFlow.Application/AgentFlow.Application.csproj", "src/AgentFlow.Application/"]
COPY ["src/AgentFlow.Contracts/AgentFlow.Contracts.csproj", "src/AgentFlow.Contracts/"]
COPY ["src/AgentFlow.Domain/AgentFlow.Domain.csproj", "src/AgentFlow.Domain/"]
COPY ["src/AgentFlow.Infrastructure/AgentFlow.Infrastructure.csproj", "src/AgentFlow.Infrastructure/"]
COPY ["src/AgentFlow.Worker/AgentFlow.Worker.csproj", "src/AgentFlow.Worker/"]
RUN dotnet restore "AgentFlow.slnx"

COPY . .
RUN dotnet publish "src/AgentFlow.Worker/AgentFlow.Worker.csproj" \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AgentFlow.Worker.dll"]
