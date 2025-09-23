## ProjectFolderOverview.md

### /Composition

Purpose:
Defines how the application is composed at startup — especially in Program.cs. It contains extension methods used to:

Register services (DI setup)

Configure middleware

Wire up features like Swagger, health checks, logging, etc.



### /Configuration

Purpose:
Handles application and environment configuration logic.

Resolves connection strings (including cloud fallbacks)

Binds app settings to strongly typed config classes 

Defines helper extensions for IConfiguration


### Middleware

Purpose:
Contains custom middleware components that intercept HTTP requests.

Examples:

* Request logging

* Error handling

* Authentication/authorization

* Metrics or timing


### /Repository

Purpose:
Encapsulates data access logic, abstracting away database interaction.

Repositories are often:

* Thin wrappers around SQL, Dapper, EF Core, etc.

* Registered as services in DI

* Called by controllers or service layers