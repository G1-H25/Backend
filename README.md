# GPS Application Chas Advance 2025 Group 1

## Table of Content

- [Assignment](#assignment)
- [Related repos](#related-repos)
- [Project description](#project-description)
- [Folder structure](#folder-structure)
- [Getting started](#getting-started)
- [Testing](#testing)
- [API Documentation](#api-documentation)

## Assignment

A school project where three classes from Chas Academy year 2 including SUVx24(Embedded), FJSx24(Fullstack) and FSWx24(Frontend).  
Chas Academy has given us the task to develop a prototype of for climatecontrolled logistics system where we can easily manage and monitor the climate during delivery.  
This product will reassure the end customer of deviations during delivery while making sure that users of the system can collect data for further improvements.  

## Related repos

Link to [Frontend-Web](https://github.com/G1-H25/Frontend-web)  
Link to [Frontend-Mobile/UX](https://github.com/G1-H25/Frontend-mobile)  
Link to [Device-Broker](https://github.com/G1-H25/Device-Broker)  
Link to [Device-Sensor](https://github.com/G1-H25/Device-Sensor)  
Link to [jenlib](https://github.com/G1-H25/jenlib)  
Link to our list of [Requirements](https://github.com/G1-H25/Requirements)

## Project description

This repo will represent the backend part of our project where we will create a communactions bridge between the sensors and UI.  
We are developing an API using C# and the ASP.NET core.
The server is hosted on Azure Data Portal where we also handle logging for the API requests.  
This is to summarize our last course (DevOps) using technologies like GitHub Actions to create a robust, secure and automatic workflow.

## Folder structure

```bash
    .
    ├── docs                    # Documents gathered under one folder
    ├── GpsApp                  # Application
    ├── GpsApp.Tests            # Tests for application (integration and unit tests)
    ├── scripts                 # Scripts for database creation
    ├── .gitattributes          # NO CLUE Wilmer?? Does something with Shell files???
    ├── .gitignore              # Configuration for what files to exclude on GitHub
    ├── .sqlfluff               # Linter for .sql files
    ├── Backend.sln             # Solution for application
    ├── docker-compose.yml      # Docker-compose to set up a local testing environment 
    └── README.md               # <---WELCOME--->
```

## Getting started

### Creating and running Docker container

### 1. Clone repository

> [!IMPORTANT]
> Make sure that you have the requirements before starting
> [Link here](docs/DEV_REQUIREMENTS.md)

```bash
git clone https://github.com/G1-H25/Backend.git
cd Backend
```

### 2. **Run Docker**

From source run `docker compose up --watch` or `docker-compose up --build`

--watch is to automatically have changes made in the source repo update the docker image.  
--build creates the image from existing files when run.

> [!WARNING]
> --watch does currently not work.

> [!TIP]
> If ports are blocked, check for other running containers that might block port usage.

### 3. **Additional tips**

For entering the backend container shell:

`docker exec -it backend-app-1 /bin/sh`

For entering the database shell

`docker run -it --rm --network container:dev-sqlserver mcr.microsoft.com/mssql-tools /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd`

### 4. **Stopping containers & clearing cache**

Run `docker-compose down -v` remove the "volume" from the docker.
If you do not run `-v`, metadata will be saved in your container and keep on existing until removed.  
This might cause collisions when you build new images.

#### Notes

- `http://localhost:5000/swagger/index.html` is used for testing endpoints.

- Make sure Docker Desktop is running in the background.

## Testing

Tests are crucial for any application to ensure the security and robustness that a system requires.  
Here is a link to how our tests are built and how we test the application.  
[TESTING.md](docs/TESTING.md)

## API Documentation

Swagger provides the ability to test different methods for retreiving or fetching different data packages. Try it out!  
[Swagger Docs](https://g1api-bgeuc6hydmg9etgt.swedencentral-01.azurewebsites.net/swagger/index.html)
[ERP Diagram | Lucidchart](https://lucid.app/lucidchart/3512ac64-3834-4b7e-b511-6808a2e46dc5/edit?page=0_0#)
