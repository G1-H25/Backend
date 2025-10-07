#!/bin/sh
set -e

USERNAME="testuser_$(date +%s)"
PASSWORD="testpass123"

echo "Signing up with username: $USERNAME"

signup_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/signup/signup \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

echo "Signup response:"
echo "$signup_response"

signup_status=$(echo "$signup_response" | tail -n1 | awk '{print $3}')

if [ "$signup_status" != "200" ]; then
  echo "ERROR: Signup failed with status $signup_status"
  exit 1
fi

echo "Logging in..."

login_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/login \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$USERNAME\",\"password\":\"$PASSWORD\"}")

echo "Login response:"
echo "$login_response"

login_status=$(echo "$login_response" | tail -n1 | awk '{print $3}')

if [ "$login_status" != "200" ]; then
  echo "ERROR: Login failed with status $login_status"
  exit 1
fi

TOKEN=$(echo "$login_response" | sed -n 's/.*"token":"\([^"]*\)".*/\1/p')

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "ERROR: Failed to extract token from login response"
  exit 1
fi

echo "Token received: $TOKEN"

echo "Testing authentication with token..."

auth_test_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X GET http://localhost:5000/test/user-only \
  -H "Authorization: Bearer $TOKEN")

echo "Auth test response:"
echo "$auth_test_response"

auth_test_status=$(echo "$auth_test_response" | tail -n1 | awk '{print $3}')

if [ "$auth_test_status" != "200" ]; then
  echo "ERROR: Authentication test failed with status $auth_test_status"
  exit 1
fi

echo "Registering device..."

register_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/Gateway \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{}")

echo "Register response:"
echo "$register_response"

register_status=$(echo "$register_response" | tail -n1 | awk '{print $3}')

if [ "$register_status" != "200" ]; then
  echo "ERROR: Device registration failed with status $register_status"
  exit 1
fi

DEVICEID=$(echo "$register_response" | grep -o "\"deviceId\":[0-9]*" | grep -o "[0-9]*")

if [ -z "$DEVICEID" ]; then
  echo "ERROR: Failed to extract device ID"
  exit 1
fi

echo "Device ID: $DEVICEID"

echo "Posting GPS data..."

post_gps_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/Gps \
  -H "Content-Type: application/json" \
  -d "{
    \"DeviceId\": \"$DEVICEID\",
    \"Latitude\": 51.509865,
    \"Longitude\": -0.118092,
    \"Timestamp\": \"2025-09-08T12:00:00Z\"
  }")

echo "Post GPS response:"
echo "$post_gps_response"

post_gps_status=$(echo "$post_gps_response" | tail -n1 | awk '{print $3}')

if [ "$post_gps_status" != "200" ]; then
  echo "ERROR: Posting GPS data failed with status $post_gps_status"
  exit 1
fi

echo "Fetching GPS data..."

fetch_gps_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X GET "http://localhost:5000/GpsGet?DeviceId=$DEVICEID" \
  -H "Authorization: Bearer $TOKEN")

echo "Fetch GPS response:"
echo "$fetch_gps_response"

fetch_gps_status=$(echo "$fetch_gps_response" | tail -n1 | awk '{print $3}')

if [ "$fetch_gps_status" != "200" ]; then
  echo "ERROR: Fetching GPS data failed with status $fetch_gps_status"
  exit 1
fi

echo "$TOKEN"
