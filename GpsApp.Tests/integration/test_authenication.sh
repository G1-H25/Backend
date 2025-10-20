#!/bin/sh
set -euxo pipefail

echo "Registering company..."

COMPANY_NAME="TestCompany_$(date +%s)"

company_response=$(curl -v -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/company/register \
  -H "Content-Type: application/json" \
  -d "{
        \"companyName\": \"$COMPANY_NAME\",
        \"email\": \"test@example.com\",
        \"address\": {
          \"street\": \"123 Test St\",
          \"streetNumber\": 10,
          \"postalCode\": \"12345\",
          \"city\": \"Testville\",
          \"country\": \"Testland\"
        }
      }")



echo "Company registration response:"
echo "$company_response"

company_status=$(echo "$company_response" | tail -n1 | awk '{print $3}')

if [ "$company_status" != "200" ]; then
  echo "ERROR: Company registration failed with status $company_status"
  exit 1
fi

COMPANY_ID=$(echo "$company_response" | sed -n 's/.*"companyId":\([^,}]*\).*/\1/p')

if [ -z "$COMPANY_ID" ] || [ "$COMPANY_ID" = "null" ]; then
  echo "ERROR: Failed to extract company ID from response"
  exit 1
fi

echo "Company ID: $COMPANY_ID"

USERNAME="testuser_$(date +%s)"
PASSWORD="testpass123"

echo "Signing up with username: $USERNAME"

signup_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/signup \
  -H "Content-Type: application/json" \
  -d "{
        \"username\":\"$USERNAME\",
        \"password\":\"$PASSWORD\",
        \"companyId\":\"$COMPANY_ID\",
        \"role\":\"User\"  
      }")

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

register_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/Gateway/register \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  --data-binary '{"deviceId":1}')

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

echo "Posting sensor data..."

sensor_post_response=$(curl -s -w "\nHTTP Status: %{http_code}\n" -X POST http://localhost:5000/Sensor \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "{
        \"gatewayId\": $DEVICEID,
        \"temperatureCel\": 22.5,
        \"humdityPct\": 55.2
      }")

echo "Sensor post response:"
echo "$sensor_post_response"

sensor_post_status=$(echo "$sensor_post_response" | tail -n1 | awk '{print $3}')

if [ "$sensor_post_status" != "200" ]; then
  echo "ERROR: Posting sensor data failed with status $sensor_post_status"
  exit 1
fi


echo "Token received: $TOKEN"