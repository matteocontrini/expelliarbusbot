SELECT
  trips.trip_id,
  trips.shape_id,
  MAX(CASE WHEN stop_times.stop_id = 150 THEN stop_times.departure_time END) departure

FROM trips, stop_times

WHERE trips.route_id = 400
  AND trips.direction_id = 1
  AND trips.trip_id = stop_times.trip_id
  AND trips.service_id IN (
    SELECT service_id FROM calendar
      WHERE thursday = 1
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

GROUP BY trips.trip_id
ORDER BY departure ASC
