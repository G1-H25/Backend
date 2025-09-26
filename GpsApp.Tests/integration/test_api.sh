#!/bin/sh

# Wait for backend to be ready on host port 5000
echo "Waiting for backend to be ready..."
for i in $(seq 1 60); do
  status_code=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health)
  
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


# Now enter the container shell and run the rest of the tests inside the container
# Replace 'backend-app-1' with your actual container name
docker exec -i backend-app-1 /bin/sh -c "
  USERNAME=\"testuser_\$(date +%s)\"
  PASSWORD=\"testpass123\"

  echo \"Signing up with username \$USERNAME...\"
  curl -i -X POST http://localhost:8080/signup/signup \
    -H \"Content-Type: application/json\" \
    -d '{\"username\": \"'\$USERNAME'\", \"password\": \"'\$PASSWORD'\"}'
  echo

  echo \"Logging in...\"
  response=\$(curl -s -X POST http://localhost:8080/login \
    -H \"Content-Type: application/json\" \
    -d '{\"username\":\"'\$USERNAME'\", \"password\":\"'\$PASSWORD'\"}')

  echo \"Raw login response:\"
  echo \"\$response\"

  TOKEN=\$(echo \"\$response\" | grep -o '\"token\":\"[^\"]*' | cut -d':' -f2 | tr -d '\"')

  if [ \"\$TOKEN\" = \"null\" ] || [ -z \"\$TOKEN\" ]; then
    echo \"Login failed, no token received\"
    exit 1
  fi

  echo \"Token received: \$TOKEN\"
  echo

  echo \"Testing auth...\"
  curl -s -X GET http://localhost:8080/test/user-only \
    -H \"Authorization: Bearer \$TOKEN\"
  echo

  echo \"Registering device...\"
  curl -s -X POST http://localhost:8080/Gateway \
    -H \"Content-Type: application/json\" \
    -H \"Authorization: Bearer \$TOKEN\" \
    -d '{\"DeviceId\": 12345}'
  echo

  echo \"Posting GPS data...\"
  curl -s -X POST http://localhost:8080/Gps \
    -H \"Content-Type: application/json\" \
    -d '{
      \"DeviceId\": \"12345\",
      \"Latitude\": 51.509865,
      \"Longitude\": -0.118092,
      \"Timestamp\": \"2025-09-08T12:00:00Z\"
    }'
  echo

  echo \"Fetching GPS data...\"
  curl -s -X GET \"http://localhost:8080/GpsGet?DeviceId=12345\" \
    -H \"Authorization: Bearer \$TOKEN\"
  echo
"
