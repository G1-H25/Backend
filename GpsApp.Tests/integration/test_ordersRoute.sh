#!/bin/sh
set -e

TOKEN=$1
echo "Using token in orders route test: $TOKEN"

echo "Fetching delivery data from API via localhost..."

delivery_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" \
  "http://localhost:5000/DeliveryGet?id=1")

echo "Delivery data response:"
echo "$delivery_response"

status_code=$(echo "$delivery_response" | tail -n1 | awk '{print $3}')

if [ "$status_code" != "200" ]; then
  echo "ERROR: Failed to fetch delivery data, status $status_code"
  exit 1
fi

echo "Data retrieved successfully."
