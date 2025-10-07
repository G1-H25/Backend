#!/bin/sh

TOKEN=$1

if [ -z "$TOKEN" ]; then
  echo "Token must be provided as first argument" >&2
  exit 1
fi

register_response=$(curl -s -X POST http://localhost:8080/Gateway \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{}")

echo "$register_response" > /tmp/register_response.json

DEVICEID=$(echo "$register_response" | grep -o "\"deviceId\":[0-9]*" | grep -o "[0-9]*")

if [ -z "$DEVICEID" ]; then
  echo "Failed to get device ID from registration" >&2
  exit 1
fi

echo "Using Device ID: $DEVICEID"

# Save for use in later scripts
echo "$DEVICEID" > /tmp/test_device_id.txt
