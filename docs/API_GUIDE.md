# Swagger Guide

## Description

Provided here, is a guide you can use to navigate through our production API Documentation([found here](https://g1api-bgeuc6hydmg9etgt.swedencentral-01.azurewebsites.net/swagger/index.html)).  

> [!WARNING]
> Production API documentation is not updated.
> Test the updated version in development localhost.

[Link here](../README.md#getting-started) to run a container and connect to `http://localhost:5000/swagger/index.html`.  

We are using Swagger because of its reliability and ability to generate automated documentation written in the source code.  
Automation is a key part of our curriculum as it also removes alot of the possible human errors that can occur.

## Guide

### 1. Get started

1.1 Run `docker-compose.yml` in root repository.

![container image](images/docker/.png)

1.2 Connect to `http://localhost:5000/swagger/index.html`.

![swagger](images/swagger/.png)

### 2. Create a Gateway

2.1 Create a valid GatewayID that will be used for sending batch readings.
![swagger gatewaycreation](images/swagger/.png)

### 3. Insert a sensor batch reading

3.1 Insert a new sensor batch with a valid UNIX timestamp, a valid id and a valid gatewayid that matches the previous inserted gateway value from batch.

### 4. Sign up an account

4.1 Create a new user/admin.
![signup account](images/swagger/.png)

4.2 Create a new company connected to your user/admin.
![create company](images/swagger/.png)

### 5. Login

5.1 Retreive a JWT token during login.
![generate JWT token](images/swagger/.png)

5.2 Locate the Authorize button in the top of the document.
![authorize JWT token](images/swagger/.png)

### 6. List deliveries

6.1 Retrive a list of all deliveries.
![list all deliveries](images/swagger/.png)

6.2 If you want to create more deliveries, jump to [step number 3](#3-insert-a-sensor-batch-reading) and insert a new sensor batch.
