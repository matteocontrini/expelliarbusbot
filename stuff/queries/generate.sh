#!/bin/bash
wget "https://www.trentinotrasporti.it/opendata/google_transit_urbano_tte.zip"
rm -rf gtfs
unzip "google_transit_urbano_tte.zip" -d gtfs
sqlite3 "gtfs_$(date '+%Y_%m_%d').db" < import.sql
