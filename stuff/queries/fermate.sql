select stop_times.departure_time, stops.stop_id, stops.stop_name
from stop_times, stops
where stop_times.stop_id = stops.stop_id
and stop_times.trip_id = "0002957882018091220190608"