#!/bin/sh

# Wait for backend to be ready (up to 20 seconds)
echo "Waiting for backend to be ready..."
for i in $(seq 1 60); do
  status_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8080/health)
  if [ "$status_code" = "200" ]; then
    echo "Backend is ready!"
    break
  else
    echo "Backend not ready yet (status $status_code), waiting..."
    sleep 2
  fi

  if [ "$i" = "60" ]; then
    echo "Backend did not become ready in time, exiting."
    exit 1
  fi
done

# step 1, create a user
USERNAME="testuser_$(date +%s)"
PASSWORD="testpass123"

echo "Signing up with username $USERNAME..."
curl -i -X POST http://localhost:8080/signup/signup \
-H "Content-Type: application/json" \
-d "{\"username\": \"$USERNAME\", \"password\": \"$PASSWORD\"}"
echo "\n"

# 2. Login and extract token
echo "Logging in..."
response=$(curl -s -X POST http://localhost:8080/login \
-H "Content-Type: application/json" \
-d "{\"username\":\"$USERNAME\", \"password\":\"$PASSWORD\"}")

echo "Raw login response:"
echo "$response"

# Extract token using grep + cut (no jq required)
TOKEN=$(echo "$response" | grep -o '"token":"[^"]*' | cut -d':' -f2 | tr -d '"')

if [ "$TOKEN" = "null" ] || [ -z "$TOKEN" ]; then
  echo "Login failed, no token received"
  exit 1
fi

echo "Token received: $TOKEN"
echo "\n"

# 3. Test authentication
echo "Testing auth..."
curl -s -X GET http://localhost:8080/test/user-only \
-H "Authorization: Bearer $TOKEN"
echo "\n"

# 4. Register device
echo "Registering device..."
curl -s -X POST http://localhost:8080/Gateway \
-H "Content-Type: application/json" \
-H "Authorization: Bearer $TOKEN" \
-d '{"DeviceId": 12345}'
echo "\n"

# 5. Post GPS data
echo "Posting GPS data..."
curl -s -X POST http://localhost:8080/Gps \
-H "Content-Type: application/json" \
-d '{
  "DeviceId": "12345",
  "Latitude": 51.509865,
  "Longitude": -0.118092,
  "Timestamp": "2025-09-08T12:00:00Z"
}'
echo "\n"

# 6. Fetch GPS data
echo "Fetching GPS data..."
curl -s -X GET "http://localhost:8080/GpsGet?DeviceId=12345" \
-H "Authorization: Bearer $TOKEN"
echo "\n"
