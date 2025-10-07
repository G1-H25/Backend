#!/bin/sh

# Wait for backend to be ready on host port 5000
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