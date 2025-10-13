#!/bin/sh
set -e

TOKEN=$1

echo "Fetching delivery data from API inside container..."

delivery_response=$(docker exec -i backend-app-1 /bin/sh -c "
  set -e
  RESPONSE=\$(curl -s -w \"\nHTTP Status: %{http_code}\n\" \\
    -H \"Authorization: Bearer $TOKEN\" \\
    http://localhost:5000/DeliveryGet)
  echo \"\$RESPONSE\"
")

echo "Delivery data response:"
echo "$delivery_response"

status_code=$(echo "$delivery_response" | tail -n1 | awk '{print $3}')

if [ \"$status_code\" != \"200\" ]; then
  echo \"ERROR: Failed to fetch delivery data, status $status_code\"
  exit 1
fi

echo "Data retrieved successfully."
