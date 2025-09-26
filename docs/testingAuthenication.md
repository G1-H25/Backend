Enter test shell
docker exec -it backend-app-1 /bin/sh

Signup an account

curl -i -X POST http://localhost:8080/signup/signup -H "Content-Type: application/json" -d '{"username": "testuser3", "password": "testpass123"}'


Logging in with the account
curl -i -X POST http://localhost:8080/login -H "Content-Type: application/json" -d '{"username":"testuser3", "password":"testpass123"}'



test if authenication worked (replace the <token> with what was provided in the previous command)

curl -i http://localhost:8080/test/user-only -H "Authorization: Bearer <token>"