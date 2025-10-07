#!/bin/sh

CONTAINER_NAME=backend-app-1
DEVICEID=$1
TOKEN=$2

if [ -z "$DEVICEID" ] || [ -z "$TOKEN" ]; then
  echo "Device ID and Token must be provided as arguments" >&2
  exit 1
fi

docker exec -i $CONTAINER_NAME /bin/sh -c "
  curl -s -X GET http://localhost:8080/GpsGet?DeviceId=$DEVICEID \
    -H 'Authorization: Bearer $TOKEN'
"
echo
