# Rhino API Reference Guide

## Getting Started
* [Create an Account (Free)](./GettingStarted/Register.md 'Register')
* [Rhino Agent Deployment - Process Host](./GettingStarted/Deployment.md 'Deployment')
* [Rhino Agent Deployment - Docker](./GettingStarted/DeploymentDocker.md 'DeploymentDocker')
* [Rhino Agent Deployment - On IIS (Internet Information Service)](./GettingStarted/DeploymentIIS.md 'DeploymentIIS')
* [Server Settings - Set Reporters and Logs Locations and Behavior](./GettingStarted/ServerSettings.md 'ServerSettings')
* [Verify Rhino Deployment - Execute a Set of Verification and Integration Tests](./GettingStarted/VerifyDeployment.md 'VerifyDeployment')
* [Install and Connect Rhino Widget (Chrome/Edge extensions)](./GettingStarted/ConnectWidget.md 'ConnectWidget')
* [Widget Overview - User Interface and Different Sections](./GettingStarted/WidgetOverview.md 'WidgetOverview')
* [Create Your First Automation](./GettingStarted/YourFirstAutomation.md 'YourFirstAutomation')
* Save Your Automation as Text File
* Save Your Automation on Application Lifecycle Manager
* Run Your Automation from a Command Line
* Run Your Automation Over HTTP
* Run Your Automation from CI/CD Task

## API Reference
* [Configurations](./ApiReference/Configurations.md 'Configurations')
* [Debug](./ApiReference/Debug.md 'Debug')
* [Environment](./ApiReference/Environment.md 'Environment')
* [Logs](./ApiReference/Logs.md 'Logs')
* [Meta Data](./ApiReference/Meta.md 'Meta')
* [Models](./ApiReference/Models.md 'Models')
* [Plugins](./ApiReference/Plugins.md 'Plugins')
* [Rhino](./ApiReference/Rhino.md 'Rhino')
* [Rhino Async](./ApiReference/RhinoAsync.md 'RhinoAsync')
* [Test Cases (Rhino Specs)](./ApiReference/Tests.md 'Tests')
* [Utilities](./ApiReference/Utilities.md 'Utilities')

## Official Connectors
* [Gurock, TestRail Connector](https://github.com/savanna-projects/rhino-connectors-gurock)
* [Atlassian, Jira with XRay (Cloud & Server) Connector](https://github.com/savanna-projects/rhino-connectors-atlassian)
* [Plain Text Connector](https://github.com/savanna-projects/rhino-connectors-text)
* [Microsoft, Azure DevOps (>= 2018) Connector](https://github.com/savanna-projects/rhino-connectors-azure)

## Rhino Worker

`Rhino Worker` is a special process that can connect to a `Rhino Agent Hub` and take payload from it, allowing distributed tests invocation.

### Worker Command Line Parameters

```bash
dotnet Rhino.Worker.dll --hubAddress:http://localhost:9000 --hubApiVersion:3 --maxParallel:1 --connectionTimeout:10
```

| Parameter           | Type   | Description                                                                                                             |
|---------------------|--------|-------------------------------------------------------------------------------------------------------------------------|
| `ConnectionTimeout` | Double | The amount of time _**in minutes**_ to retry syncing with `Rhino Hub` before the connection is terminated.              |
| `hubAddress`        | String | The public address of the hub (`Rhino Agent`) including the port (e.g., `http://localhost:9000`).                       |
| `hubApiVersion`     | String | The API version of the hub (e.g., `3`).                                                                                 |
| `maxParallel`       | Number | The maximum connections that will be opened by the `Rhino Worker` this allows each worker to run more than one process. |

### Wroker Settings

These settings can be found in `appsettings.json` file under `Rhino > WorkerConfiguration` node.

| Parameter           | Type   | Description                                                                                                             |
|---------------------|--------|-------------------------------------------------------------------------------------------------------------------------|
| `ConnectionTimeout` | Double | The amount of time _**in minutes**_ to retry syncing with `Rhino Hub` before the connection is terminated.              |
| `HubAddress`        | String | The public address of the hub (`Rhino Agent`) including the port (e.g., `http://localhost:9000`).                       |
| `HubApiVersion`     | String | The API version of the hub (e.g., `3`).                                                                                 |
| `MaxParallel`       | Number | The maximum connections that will be opened by the `Rhino Worker` this allows each worker to run more than one process. |

### Worker Docker Environment Parameters

| Parameter            | Type   | Description                                                                                                             |
|----------------------|--------|-------------------------------------------------------------------------------------------------------------------------|
| `CONNECTION_TIMEOUT` | Double | The amount of time _**in minutes**_ to retry syncing with `Rhino Hub` before the connection is terminated.              |
| `HUB_ADDRESS`        | String | The public address of the hub (`Rhino Agent`) including the port (e.g., `http://localhost:9000`).                       |
| `HUB_API_VERSION`    | String | The API version of the hub (e.g., `3`).                                                                                 |
| `MAX_PARALLEL`       | Number | The maximum connections that will be opened by the `Rhino Worker` this allows each worker to run more than one process. |
