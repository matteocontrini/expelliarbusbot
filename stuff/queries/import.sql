-- enable CSV import mode
.mode csv

-- define the CSV separator
.separator ','

-- import & create tables, this will not insert the header line of a CSV file as a row
.import gtfs/agency.txt agencies
.import gtfs/calendar.txt calendar
.import gtfs/calendar_dates.txt calendar_dates
.import gtfs/routes.txt routes
.import gtfs/shapes.txt shapes
.import gtfs/stops.txt stops
.import gtfs/stop_times.txt stop_times
.import gtfs/transfers.txt transfers
.import gtfs/trips.txt trips

-- create indexes
CREATE INDEX "stop_times_trip_id" ON "stop_times" (
	"trip_id"
);

CREATE INDEX "trips_route_id" ON "trips" (
	"route_id"
);
