Enter test shell
docker exec -it backend-app-1 /bin/sh

Signup an account

curl -i -X POST http://localhost:8080/signup/signup -H "Content-Type: application/json" -d '{"username": "testuser3", "password": "testpass123"}'


Logging in with the account
curl -i -X POST http://localhost:8080/login -H "Content-Type: application/json" -d '{"username":"testuser3", "password":"testpass123"}'



testing if authenication worked

curl -i http://localhost:8080/test/user-only -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ0ZXN0dXNlcjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdHVzZXIxIiwiaHR0cDovL3NjaGVtYXMubWljcm9zb2Z0LmNvbS93cy8yMDA4LzA2L2lkZW50aXR5L2NsYWltcy9yb2xlIjoiVXNlciIsImp0aSI6ImZjNTdjZDFiLTBjMzctNGI2OC1iOWI1LTVkNjdlYmYxNzNhNSIsImV4cCI6MTc1ODc5OTM0MCwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDgwLyIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6ODA4MC8ifQ.6t4Dn_mpgvgRtA7tyBD5K94zcweeKXOPtyEZQPq0Sis"