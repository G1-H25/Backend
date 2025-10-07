#!/bin/sh

TOKEN=$1

if [ -z "$TOKEN" ]; then
  echo "Token must be provided as first argument" >&2
  exit 1
fi

echo "Registering device with token: $TOKEN"
echo


register_response=$(curl -s -X POST http://localhost:8080/Gateway \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{}")

echo "Register response: $register_response"


DEVICEID=$(echo "$register_response" | sed -n 's/.*"deviceId":\([0-9]*\).*/\1/p')


if [ -z "$DEVICEID" ]; then
  echo " Failed to extract device ID from response" >&2
  exit 1
fi

echo "Using Device ID: $DEVICEID"

# Save for use in later scripts
echo "$DEVICEID" > /tmp/test_device_id.txt
