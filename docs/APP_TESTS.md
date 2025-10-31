# Testing

## Descr

[Swagger](https://g1api-bgeuc6hydmg9etgt.swedencentral-01.azurewebsites.net/swagger/index.html)

## Test folder structure

```bash
GpsApp.Tests                                        # Test root folder
├── integration                                     # Integration subfolder
│   ├── AuthenicationIntegrationTests.cs            # Tests the API and authenticity of users / admins
│   ├── GpsApp.tests.Integration.csproj             # Config for integration tests folder
│   └── TestFixture.cs                              # Determines localhost:port for testing
└── unit                                            # Unit subfolder
    ├── AuthorizationServiceHasAccessToDevice.cs    # Authentication tests for users /admins 
    ├── GpsApp.Tests.Unit.csproj                    # Config for unit tests folder 
    └── HelloControllerTests.cs                     # Hello test for controllers
```

## Description

This is a simpler guide of testing the program itself and ensure its compatibility.
We are using the [Xunit](https://xunit.net/?tabs=cs) framework for both integration and unit tests while developing.

## Intergration Tests

### Command for running integration test

> **CAUTION:** *When run locally, test will complain if Docker is not up first.**

To set up Docker environment visit README in root folder. [Link to README](../README.md#testing)  

```bash
dotnet test GpsApp.Tests/integration                # Runs integration tests
```

#### AuthenicationIntegrationTests.cs

Tests the authentication that an admin / user should have. This involves setup, login, token testing and registration of devices to a certain user.

#### TestFixture.cs

Sets the address `"http://localhost:5000"` so that others tests can run through this URI and connect to container.

## Unit tests

Unit tests are supposed to test smaller individual crucial parts that can be logically **isolated** in an application.  
This could include the testing of either a function, module, method or class.

### Command for running unit tests

```bash
dotnet test GpsApp.Tests/unit                       # Runs unit tests
```

#### AuthorizationServiceHasAccessToDevice.cs

#### HelloControllerTests.cs

Validates that unit tests are working and responds with a "Hello" message if the connection is valid.
