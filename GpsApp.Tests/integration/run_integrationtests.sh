#!/bin/sh
set -e

echo "Step 1: Waiting for backend to be ready..."
./wait_for_backend.sh

echo "Step 2: Signing up and logging in..."
TOKEN=$(./signup_and_login.sh)
echo "Received Token: $TOKEN"

echo "Step 3: Registering device..."
DEVICEID=$(./register_device.sh "$TOKEN")
echo "Received Device ID: $DEVICEID"

echo "Step 4: Posting GPS data..."
./post_gps_data.sh "$DEVICEID"

echo "Step 5: Fetching GPS data..."
./fetch_gps_data.sh "$DEVICEID" "$TOKEN"

echo "All integration tests completed successfully!"
