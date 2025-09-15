# GPS Application Chas Advance 2025 Group 1

## Table of Content

- [Assignment](#Assigment)
- [Related repos](#Related-repos)
- [Project description](#Project-description) 
- [Project structure](#Projects-structure)
- [Getting started](#Getting-started)
- [Testing](#Testing)
- [API Documentation](#API-Documentation)
- [License](#License)

### Assignment

A school project where three classes from Chas Academy year 2 including SUVx24(Embedded), FJSx24(Fullstack) and FSWx24(Frontend). Chas Academy has given us the task to develop a prototype of a system for climatecontrolled transportation to different potentional industries like, food, medicin, chemical and other industries where climate control is needed.

### Related repos

Link to [Frontend-Web](https://github.com/G1-H25/Frontend-web)
Link to [Frontend-Mobile/UX](https://github.com/G1-H25/Frontend-mobile)
Link to [Device-Broker](https://github.com/G1-H25/Device-Broker)
Link to [Device-Sensor](https://github.com/G1-H25/Device-Sensor)
Link to our list of [Requirements](https://github.com/G1-H25/Requirements)(written in Swedish)

### Project description

This repo will represent the backend (such as API, Server and Database) part of the project where we will handle the communication between the devices and users.  
Our goal is to develop a structured API with the ASP.NET core while using a GitHub Actions workflow.  
The server is going to be hosted on Azure cloud services where we also will use the AzureSQL for our database.  
We are going to summarize our last course (DevOps) using technologies like GitHub Actions to create a robust, secure and automatic workflow. 

### Project structure

´´´csharp
Provide folder structure later, when it is determined. 
´´´

### Getting started

Here you will find a link/description on how all the needed dependencies are installed and waht you will need to run this program. The backend is not the only part 

### Testing

Here you will find a link/description on what library will be used for testing and how the process is made

### Running it locally

1. Clone the repo

`git clone https://github.com/G1-H25/Backend.git`
`cd backend`

2. run docker

Either. `docker compose up --watch` or `docker-compose up --build`

watch is to automatically have changes in the file update the docker, while non watch does not do this.

docker watch currently does not work

3. additional tips

to enter the backend container shell

- `docker exec -it backend-app-1 /bin/sh`

to enter database shell

`docker run -it --rm --network container:dev-sqlserver mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd`

4. Start coding 

5. Stopping & Cleaning Up

Stop containers, clear cache volumes:

`docker-compose down -v`

Notes

Make sure Docker Desktop is running

If using --watch, ensure you're on a compatible Docker version

If ports are blocked, check for running containers:

### API Documentation

Here you will find a link/description to how we have decided to structure our API and possibly our database.
[Link to API_DOCUMENTATION](provide link)

### License 

MIT License

Copyright 2025 G1-H25 Organisation

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE