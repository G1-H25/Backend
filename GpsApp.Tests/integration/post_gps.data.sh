#!/bin/sh

CONTAINER_NAME=backend-app-1
DEVICEID=$1

if [ -z "$DEVICEID" ]; then
  echo "Device ID must be provided as first argument" >&2
  exit 1
fi

docker exec -i $CONTAINER_NAME /bin/sh -c "
  curl -s -X POST http://localhost:8080/Gps \
    -H 'Content-Type: application/json' \
    -d '{
      \"DeviceId\": \"$DEVICEID\",
      \"Latitude\": 51.509865,
      \"Longitude\": -0.118092,
      \"Timestamp\": \"2025-09-08T12:00:00Z\"
    }'
"

echo "GPS data posted."
