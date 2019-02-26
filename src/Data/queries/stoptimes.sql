SELECT stop_times.departure_time, stops.stop_id, stops.stop_name
FROM stop_times, stops
WHERE stop_times.stop_id = stops.stop_id
AND stop_times.trip_id = ?
