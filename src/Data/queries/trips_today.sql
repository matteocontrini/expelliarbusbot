SELECT
  trips.trip_id,
  trips.shape_id,
  (SELECT departure_time FROM stop_times WHERE trip_id = trips.trip_id AND stop_id = 150) departure

FROM trips

WHERE trips.route_id = 400
  AND trips.direction_id = 1
  AND trips.service_id IN (
    SELECT service_id FROM calendar
      WHERE {dayOfWeek} = 1
        AND start_date <= ?
        AND end_date >= ?
      UNION
        SELECT service_id FROM calendar_dates
          WHERE date = ?
          AND exception_type = 1
      EXCEPT
        SELECT service_id FROM calendar_dates
          WHERE date = ?
            AND exception_type = 2
    )

ORDER BY departure ASC
