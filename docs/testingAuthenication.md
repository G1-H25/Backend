Enter test shell
docker exec -it backend-app-1 /bin/sh

Signup an account

curl -i -X POST http://localhost:8080/signup/signup -H "Content-Type: application/json" -d '{"username": "testuser5", "password": "testpass123"}'


Logging in with the account
curl -i -X POST http://localhost:8080/login -H "Content-Type: application/json" -d '{"username":"testuser5", "password":"testpass123"}'



test if authenication worked (replace the <token> with what was provided in the previous command)

curl -i http://localhost:8080/test/user-only -H "Authorization: Bearer <token>"



register a device
curl -X POST http://localhost:8080/Gateway \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <token>" \
-d '{
  "DeviceId": 12345
}'


Fetch GPS data with filters. Example querying by DeviceId:
curl -X GET "http://localhost:8080/GpsGet?DeviceId=device123" \
-H "Authorization: Bearer <token>"


Post a value in
curl -X POST http://localhost:8080/Gps \
-H "Content-Type: application/json" \
-d '{
"DeviceId": "device123",
"Latitude": 51.509865,
"Longitude": -0.118092,
"Timestamp": "2025-09-08T12:00:00Z"
}'


docker cp test_api.sh backend-app-1:/test_api.sh
docker exec -it backend-app-1 sh /test_api.sh
