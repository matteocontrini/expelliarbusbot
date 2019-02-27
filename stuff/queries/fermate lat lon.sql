select stops.stop_name, stops.stop_lat, stops.stop_lon
from stop_times, stops
where stop_times.stop_id = stops.stop_id
and stop_times.trip_id = "0002949472018091220190608"
