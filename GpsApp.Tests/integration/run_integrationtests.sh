#!/bin/sh
set -ex

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "Step 1: Waiting for backend to be ready..."
"$SCRIPT_DIR/wait_for_backend.sh"

echo "Step 2: Signing up and logging in..."
OUTPUT=$("$SCRIPT_DIR/test_authenication.sh")
echo "$OUTPUT"
# Extract the token from the output
TOKEN=$(echo "$OUTPUT" | grep "Token received:" | awk '{print $3}')
echo "Token: $TOKEN"
echo "Test_Authenication.sh passed"
echo ""

echo "Step 3: Running orders route tests..."
ORDERS_OUTPUT=$("$SCRIPT_DIR/test_ordersRoute.sh" "$TOKEN")
echo "$ORDERS_OUTPUT"
# You can add checks here if your orders test script outputs something you can grep
echo "test_ordersRoute.sh passed"
echo ""

echo "All integration tests completed successfully!"


