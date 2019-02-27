select  trips.trip_id,
		trips.shape_id,
		--group_concat(stop_times.departure_time) as times,
		--count(stop_times.departure_time) as stops_count,
		max(case when stop_times.stop_id = 150 then stop_times.departure_time end) valoni
		
from	trips, stop_times, calendar, calendar_dates
-- linea 5
where 	trips.route_id = 400
-- direzione Trento
and		trips.trip_headsign = 'Piazza Dante "Dogana"'
and		trips.trip_id = stop_times.trip_id
and		trips.service_id = calendar.service_id
-- giorno corrente
and		calendar.monday = 1 -- todo improve and consider calendar_dates

group by trips.trip_id
order by valoni asc
