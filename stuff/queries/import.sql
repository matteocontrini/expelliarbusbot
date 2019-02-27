-- enable CSV import mode
.mode csv

-- define the CSV separator
.separator ','

-- import & create tables, this will not insert the header line of a CSV file as a row
.import agency.txt agencies
.import calendar.txt calendar
.import calendar_dates.txt calendar_dates
.import routes.txt routes
.import shapes.txt shapes
.import stops.txt stops
.import stop_times.txt stop_times
.import stopslevel.txt stopslevel
.import transfers.txt transfers
.import trips.txt trips
