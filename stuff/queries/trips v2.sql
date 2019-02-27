select  trips.trip_id,
		trips.shape_id,
		--group_concat(stop_times.departure_time) as times,
		--count(stop_times.departure_time) as stops_count,
		-- valoni time
		max(case when stop_times.stop_id = 150 then stop_times.departure_time end) departure
		
from	trips, stop_times
-- linea 5
where 	trips.route_id = 400
-- direzione Trento
and		trips.trip_headsign = 'Piazza Dante "Dogana"'
and		trips.trip_id = stop_times.trip_id

-- controllo calendario
-- https://stackoverflow.com/a/24100803/1633924
and		trips.service_id in (
		SELECT service_id FROM calendar
          WHERE thursday = 1
			AND start_date <= 20190425
            AND end_date >= 20190425
          UNION
            SELECT service_id FROM calendar_dates
              WHERE date = strftime('%Y%m%d', date('now'))
                AND exception_type = 1
          EXCEPT
            SELECT service_id FROM calendar_dates
              WHERE date = strftime('%Y%m%d', date('now'))
                AND exception_type = 2
		)

group by trips.trip_id
order by departure asc
