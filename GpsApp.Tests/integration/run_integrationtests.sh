#!/bin/sh
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

echo "Step 1: Waiting for backend to be ready..."
"$SCRIPT_DIR/wait_for_backend.sh"

echo "Step 2: Signing up and logging in..."
TOKEN=$("$SCRIPT_DIR/test_authenication.sh")

echo "All integration tests completed successfully!"

