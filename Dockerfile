FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /app

# Copy sln and csproj and try to restore dependencies
COPY *.sln .
COPY src/Bot/*.csproj ./src/Bot/
RUN dotnet restore

# Copy all srcs and compile
COPY . .
WORKDIR /app/Bot
RUN dotnet build

FROM build AS publish
WORKDIR /app/Bot
RUN dotnet publish -c Release -o out

FROM microsoft/dotnet:2.2-runtime-alpine AS runtime
WORKDIR /app
COPY --from=publish /app/Bot/out ./
ENTRYPOINT ["dotnet", "Bot.dll"]
