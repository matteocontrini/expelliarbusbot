# ExpelliarbusBot [![Build Status](https://travis-ci.com/matteocontrini/expelliarbusbot.svg?branch=master)](https://travis-ci.com/matteocontrini/expelliarbusbot)

[Screenshots](stuff/screenshots)

## Preparation

- Clone the repository
- Download [`google_transit_urbano.zip`](https://www.trentinotrasporti.it/opendata/google_transit_urbano_tte.zip) and extract the contents in `stuff/queries/gtfs`
- Run `sqlite3 < import.sql` in `stuff/queries` to create a database with GTFS data

Consider that GTFS data from Trentino Trasporti is updated without notice, so this needs to be repeated every now and then.

## Configuration

Configuration of the application is done through the `appsettings.json` file read from the current working directory at startup.

Examples for [development](https://github.com/matteocontrini/expelliarbusbot/blob/master/src/Bot/appsettings.example.development.json) and [production](https://github.com/matteocontrini/expelliarbusbot/blob/master/src/Bot/appsettings.example.json) environments are available.

## Running for development

Make sure that you have the .NET Core 2.2 SDK and runtime installed on your system.

### Visual Studio

Make sure that the `src\Bot\bin\Debug\netcoreapp2.2` directory contains the `appsettings.json` file and the `gtfs.db` file created above.

Run with the nice green button.

### dotnet CLI

Make sure that the `src\Bot` directory contains the `appsettings.json` file and the `gtfs.db` file created above.

Run with the dotnet CLI by executing:

```sh
cd src/Bot
dotnet run
```

### Docker Compose

A development Docker Compose file would look like this:

```yaml
version: '3'

services:
  expelliarbusbot:
    container_name: 'expelliarbusbot'
    build: .
  network_mode: 'host'
  volumes:
    - ./src/Bot/appsettings.json:/app/appsettings.json
    - ./src/Bot/db:/app/db
```

This time make sure that the configuration file lies at `src/Bot/appsettings.json`, and put the `gtfs.db` file in `src/Bot/db/gtfs.db`.

Also update the `appsettings.json` file so that the databases paths are `db/gtfs.db` and `db/bot.db`.

Now run this command in the repository directory:

```sh
docker-compose -f docker-compose.yml up --build
```

**NOTE**: mapping a dedicated `db` directory is required for database persistence. SQLite  writes temporary files other than the `.db` one, and it does't always clean them before shut down.

## Running in production

A basic Docker Compose file for production looks like this:

```yaml
version: '3'

services:
  expelliarbusbot:
    container_name: 'expelliarbusbot'
    image: 'matteocontrini/expelliarbusbot'
    restart: unless-stopped
    network_mode: 'host'
    volumes:
      - ./appsettings.json:/app/appsettings.json
      - ./db:/app/db
```

