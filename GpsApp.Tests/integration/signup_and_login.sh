#!/bin/sh

CONTAINER_NAME=backend-app-1
USERNAME="testuser_$(date +%s)"
PASSWORD="testpass123"

echo "Signing up with username $USERNAME..."
docker exec -i $CONTAINER_NAME /bin/sh -c "
  curl -i -X POST http://localhost:8080/signup/signup \
    -H 'Content-Type: application/json' \
    -d '{\"username\": \"$USERNAME\", \"password\": \"$PASSWORD\"}'
"

echo "Logging in..."
response=$(docker exec -i $CONTAINER_NAME /bin/sh -c "
  curl -s -X POST http://localhost:8080/login \
    -H 'Content-Type: application/json' \
    -d '{\"username\":\"$USERNAME\", \"password\":\"$PASSWORD\"}'
")

echo "Raw login response:"
echo "$response"

TOKEN=$(echo "$response" | grep -o "\"token\":\"[^\"]*" | cut -d: -f2 | tr -d "\"")

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "Login failed, no token received" >&2
  exit 1
fi

echo "Token received: $TOKEN"
echo "$TOKEN"
