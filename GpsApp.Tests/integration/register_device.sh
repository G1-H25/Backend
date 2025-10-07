#!/bin/sh

CONTAINER_NAME=backend-app-1
TOKEN=$1

if [ -z "$TOKEN" ]; then
  echo "Token must be provided as first argument" >&2
  exit 1
fi

register_response=$(docker exec -i $CONTAINER_NAME /bin/sh -c "
  curl -s -X POST http://localhost:8080/Gateway \
    -H 'Content-Type: application/json' \
    -H 'Authorization: Bearer $TOKEN' \
    -d '{}'
")

echo "Register response: $register_response"

DEVICEID=$(echo "$register_response" | grep -o "\"deviceId\":[0-9]*" | grep -o "[0-9]*")

if [ -z "$DEVICEID" ]; then
  echo "Failed to get device ID from registration" >&2
  exit 1
fi

echo "Using Device ID: $DEVICEID"
echo "$DEVICEID"
