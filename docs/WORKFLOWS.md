# Github Workflows

## Description

This thread will summarize and briefly describe what each workflow does. 

### Database deployment

Link: [deploy-database.yaml](../.github/workflows/deploy-database.yaml)

**Workflow**:

    * Runs on: ubuntu-22.04
    * Install needed SQLCMD needed for Azure deployment
    * Locates the sql folder structure and runs all scripts using defined Azure secrets.
    * Disrupts if any errors occur during building
    * Runs every .sql file in [scripts](../scripts/sql/) folder.

### App deployment

Link: [deploy.yaml](../.github/workflows/deploy.yaml)

**Workflow**:

    * Runs on: ubuntu-latest
    * Sets up a .NET Core project.
    * Builds pre defiened configuration
    * Deploys to Azure Web App using repo secrets.


### Integration test

Link: [integration-tests.yml](../.github/workflows/integration-tests.yml)

**Workflow**:

    * Runs on: ubuntu-latest
    * Install docker-compose
    * Locates needed folders for the integration
    * Tests and builds new updates
    * Starts docker services
    * Connect to backend servers
    * Run integrations test and uploads if successful

### C# Linter

Link: [LintingCommitChanges.yaml](../.github/workflows/LintingCommitChanges.yaml)

**Workflow**:

    * Runs on: ubuntu-latest
    * Set up a .NET Core project
    * Uses format defined in GpsApp
    * Tells user to fix format if it fails

### Unit tests

Link: [Unit-tests.yaml](../.github/workflows/Unit-tests.yaml)

**Workflows**:

    * Runs on: ubuntu-latest
    * Set up a .NET Core project
    * Builds the project from configuration
    * Runs test with logging under GpsApp.Tests folder
