# Authentication testing

## Explanation

Here you can find the authentication testing routes to test our API's access-control

## Enter test shell

`docker exec -it backend-app-1 /bin/sh`

### Signup an account

```bash
curl -i -X POST http://localhost:8080/signup/signup -H "Content-Type: application/json" -d '{"username": "testuser5", "password": "testpass123"}'
```

### Logging in with the account

```bash
curl -i -X POST http://localhost:8080/login -H "Content-Type: application/json" -d '{"username":"testuser5", "password":"testpass123"}'
```

### After successful login

If authenication worked, replace the <token> with the response that was provided from the previous command.

```bash
curl -i http://localhost:8080/test/user-only -H "Authorization: Bearer <token>"
```

### Register a device

```bash
curl -X POST http://localhost:8080/Gateway \
-H "Content-Type: application/json" \
-H "Authorization: Bearer <token>" \
-d '{
  "DeviceId": 12345
}'
```

## Fetch GPS data using filters

### Example querying by DeviceId

```bash
curl -X GET "http://localhost:8080/GpsGet?DeviceId=device123" \
-H "Authorization: Bearer <token>"
```

### Post a value in

```bash
curl -X POST http://localhost:8080/Gps \
-H "Content-Type: application/json" \
-d '{
"DeviceId": "device123",
"Latitude": 51.509865,
"Longitude": -0.118092,
"Timestamp": "2025-09-08T12:00:00Z"
}'
```
