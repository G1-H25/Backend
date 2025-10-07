#!/bin/sh
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "Step 1: Waiting for backend to be ready..."
"$SCRIPT_DIR/wait_for_backend.sh"

echo "Step 2: Signing up and logging in..."
TOKEN=$("$SCRIPT_DIR/signup_and_login.sh")
echo "Received Token: $TOKEN"

echo "Step 3: Registering device..."
DEVICEID=$("$SCRIPT_DIR/register_device.sh" "$TOKEN")
echo "Received Device ID: $DEVICEID"

echo "Step 4: Posting GPS data..."
"$SCRIPT_DIR/post_gps_data.sh" "$DEVICEID"

echo "Step 5: Fetching GPS data..."
"$SCRIPT_DIR/fetch_gps_data.sh" "$DEVICEID" "$TOKEN"

echo "All integration tests completed successfully!"

