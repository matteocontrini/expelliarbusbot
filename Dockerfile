FROM mcr.microsoft.com/dotnet/core/sdk:2.2.401 AS build
WORKDIR /app

# Copy sln and csproj and try to restore dependencies
COPY *.sln .
COPY src/Bot/*.csproj ./src/Bot/
COPY src/Data/*.csproj ./src/Data/
COPY src/MapsGenerator/*.csproj ./src/MapsGenerator/
COPY src/CustomConsoleLogger/*.csproj ./src/CustomConsoleLogger/
RUN dotnet restore

# Copy all srcs and compile
COPY . .
WORKDIR /app/src/Bot
RUN dotnet build

FROM build AS publish
WORKDIR /app/src/Bot
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/runtime:2.2 AS runtime
WORKDIR /app
COPY --from=publish /app/src/Bot/out ./
ENTRYPOINT ["dotnet", "Bot.dll"]
