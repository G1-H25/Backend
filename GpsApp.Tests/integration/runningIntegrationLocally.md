# Copy all your scripts to /tmp in the container
docker exec backend-app-1 mkdir -p /tmp/GpsApp.Tests/integration

docker cp GpsApp.Tests/integration/. backend-app-1:/tmp/GpsApp.Tests/integration/

docker exec -it backend-app-1 /bin/sh

cd /tmp/GpsApp.Tests/integration

chmod +x *.sh

./run_integrationtests.sh

exit
