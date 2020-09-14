# API: Configurations
Use the following API methods to request details about _**Rhino Configurations**_ and to create or modify them.

## Get Configurations
Returns a list of available _**Rhino Configurations**_.

```
GET /api/v3/configurations
```

#### Response Content
```js
{
    "data": {
        "configurations": [
        {
            "id": "03d1cd94-5e38-43d8-b010-e932d92f9067",
            "models": [
                "7adc7914-2bfe-41f0-9808-422bab5c412b"
            ],
            "tests": [
                "8bed8025-3cgf-52g1-0919-533cbc6d523c"
            ]
        },
        ...
    ]}
}
```

The example response includes one configuration group, with one elements collection and one tests collection.

|Name    |Type  |Description                                                              |
|--------|------|-------------------------------------------------------------------------|
|id      |string|The ID of the _**Rhino Configuration**_.                                 |
|models  |array |All available _**Rhino Page Models**_ for this _**Rhino Configuration**_.|
|tests   |array |All available _**Tests Cases**_ for this _**Rhino Configuration**_.      |

### Response Codes
|Code|Description                                                                |
|----|---------------------------------------------------------------------------|
|200 |Success, the _**Rhino Collections**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                          |

## Get Configuration
Returns an existing _**Rhino Configuration**_.

```
GET /api/v3/configurations/:configuration_id
```

|Name            |Type  |Description                             |
|----------------|------|----------------------------------------|
|configuration_id|string|The ID of the _**Rhino Configuration**_.|

### Response Content
Please see below for a typical response:

```js
{
  "name": "Rhino Automation - Chrome & Firefox",
  "testsRepository": [
    "8bed8025-3cgf-52g1-0919-533cbc6d523c"
  ],
  "driverParameters": [
    {
      "driver": "ChromeDriver",
      "driverBinaries": "http://localhost:4444/wd/hub"
    },
    {
      "driver": "FirefoxDriver",
      "driverBinaries": "http://localhost:4444/wd/hub"
    }
  ],
  "dataSource": [],
  "models": [
    "7adc7914-2bfe-41f0-9808-422bab5c412b"
  ],
  "connector": "connector_xray",
  "gravityEndpoint": "",
  "authentication": {
    "password": "<rhino_user>",
    "userName": "<rhino_password>"
  },
  "engineConfiguration": {
    "maxParallel": 1,
    "failOnException": false,
    "optimalThreshold": 3,
    "qualityThreshold": 0,
    "toleranceThreshold": 0,
    "priority": 0,
    "severity": 0,
    "errorOnExitCode": 0,
    "elementSearchingTimeout": 15000,
    "pageLoadTimeout": 60000,
    "retrunExceptions": true,
    "returnPerformancePoints": true,
    "returnEnvironment": true,
    "terminateOnAssertFailure": false
  },
  "screenshotsConfiguration": {
    "keepOriginal": false,
    "returnScreenshots": false,
    "screenshotsOut": "<path_to_screenshots_folder>",
    "onExceptionOnly": false
  },
  "reportConfiguration": {
    "reportOut": "<path_to_reports_folder>",
    "logsOut": "<path_to_logs_folder>",
    "reporters": null,
    "connectionString": null,
    "dataProvider": null,
    "archive": false,
    "localReport": true,
    "addGravityData": true
  },
  "providerConfiguration": {
    "collection": "http://localhost:8080",
    "password": "admin",
    "user": "admin",
    "project": "XDP",
    "bugManager": true,
    "capabilities": {
      "bucketSize": 15,
      "dryRun": false
    }
  }
}
```

The following system fields are always included in the response:

#### General
|Name                                                  |Type  |Description                                                                                            |
|------------------------------------------------------|------|-------------------------------------------------------------------------------------------------------|
|name                                                  |string|The name of this _**Rhino Configuration**_.                                                            |
|testsRepository                                       |array |A collection of folders and files in which there are _**Rhino Test Cases**_.                           |
|driverParameters                                      |array |A collection of parameters which represents the target platforms on which the tests will run.          |
|dataSource                                            |array |A collection of data objects which will be cascaded as primary table for all _**Rhino Test Cases**_.   |
|models                                                |array |A collection of _**Rhino Page Models**_ sources.                                                       |
|connector                                             |string|Connector implementation type to use with this _**Rhino Configuration**_.                              |
|gravityEndpoint                                       |string|Gravity Server endpoint. Use to send requests using remote gravity service instead of embedded service.|
|[authentication](#authentication)                     |string|User name and password for authentication on Rhino Service.                                            |
|[engineConfiguration](#engine-configuration)          |object|Configure the automation engine behavior.                                                              |
|[screenshotsConfiguration](#screenshots-configuration)|object|Configure the screenshot behavior.                                                                     |
|[reportConfiguration](#report-configuration)          |object|Configure the reporting behavior.                                                                      |
|[providerConfiguration](#provider-configuration)      |object|Configure the behavior against 3rd party automation provider such as Jira, Test Rail or Azure DevOps.  |

#### Authentication
|Name    |Type  |Description                 |
|--------|------|----------------------------|
|userName|string|A valid Rhino API user name.|
|password|string|A valid Rhino API password. |

#### Engine Configuration
|Name                    |Type   |Description                                                                                                            |
|------------------------|-------|-----------------------------------------------------------------------------------------------------------------------|
|maxParallel             |number |The maximum number of tests that will be executed in parallel.                                                         |
|failOnException         |boolean|When set to true, test cases will fail if exceptions were thrown during test regardless of assertions passed or failed.|
|optimalThreshold        |decimal|Any test which violates this threshold (in minutes), will be marked as non-optimal.                                    |
|qualityThreshold        |decimal|Any test which violates this threshold (in percents), will be marked as failed.                                        |
|toleranceThreshold      |decimal|Any test which falls within this threshold (in percents), will be marked as warning when fail.                         |
|priority                |number |All tests with priority lower than this number will be marked as warning when fail.                                    |
|severity                |number |All tests with severity lower than this number will be marked as warning when fail.                                    |
|errorOnExitCode         |number |The error code (console application error code) which will cause the CI/CD process to fail.                            |
|elementSearchingTimeout |number |The timeout in millisecond when searching for elements.                                                                |
|pageLoadTimeout         |number |The timeout in millisecond when loading a page or application.                                                         |
|retrunExceptions        |boolean|When set to false, exceptions will not be returned by Gravity engine. This might affect the tests results.             |
|returnPerformancePoints |boolean|When set to false, performance data will not be returned by Gravity engine. This might affect the tests results.       |
|returnEnvironment       |boolean|When set to true, will return the current Gravity Environment parameters.                                              |
|terminateOnAssertFailure|boolean|When set to true, automation will stop if assertion any assertion action failed.                                       |
|Integration             |string |3rd party platform integration. Available integrations are, BrowserStack and LambdaTest.                               |

#### Screenshots Configuration
|Name             |Type   |Description                                                                                               |
|-----------------|-------|----------------------------------------------------------------------------------------------------------|
|keepOriginal     |boolean|When set to true, will keep the original file created by Gravity engine, when creating a new Rhino report.|
|onExceptionOnly  |boolean|When set to true, returns a screenshot only if exception was thrown during execution.                     |
|returnScreenshots|boolean|When set to false, screenshots will be returned from Gravity engine.                                      |
|screenshotsOut   |decimal|The directory in which to save automatic screenshots.                                                     |

#### Report Configuration
|Name            |Type   |Description                                                                                               |
|----------------|-------|----------------------------------------------------------------------------------------------------------|
|reportOut       |string |The directory in which to save reports.                                                                   |
|logsOut         |string |The directory in which to save logs.                                                                      |
|reporters       |array  |Reporters implementations to use with this configuration.                                                 |
|connectionString|string |The reporter connection string (if needed or used by the provided implementations).                       |
|dataProvider    |string |The reporter data provider (if needed or used by provided implementations).                               |
|archive         |boolean|When set to true, will archive the report out folder as zip file and delete the original folder.          |
|localReport     |boolean|When set to false, will not generate Rhino report.                                                        |
|addGravityData  |boolean|When set to true, will save Gravity API requests and response along with the reports and logs information.|

### Response Codes
|Code|Description                                                                          |
|----|-------------------------------------------------------------------------------------|
|200 |Success, the _**Configuration**_ was returned as part of the response.               |
|404 |Not Found, the _**Configuration**_ was not found under the configurations collection.|
|500 |Fail, the server encountered an unexpected error.                                    |

#### Provider Configuration
|Name        |Type   |Description                                                                                                                                       |
|------------|-------|--------------------------------------------------------------------------------------------------------------------------------------------------|
|collection  |string |The server base address under which the application is hosted (i.e. Jira or DevOps server endpoint).                                              |
|password    |string |A valid password for your application (i.e. Jira or DevOps password).                                                                             |
|user        |string |A valid user for your application (i.e. Jira or DevOps password). The use must have create permissions for **Tests**, **Bugs** and **Executions**.|
|project     |string |The project name or ID (depends on the connector implementation) under which to find and execute tests.                                           |
|bugManager  |boolean|Set to **true** in order to activate the bug manager feature for the selected connector.                                                          |
|capabilities|object |A set of key/value for passing explicit settings and parameters to your automation provider connector.                                            |

## Create Configuration
Creates a new _**Rhino Configuration**_.

```
POST /api/v3/configurations
```

### Request Fields
The request body follows the same format as [Get Configuration](#get-configuration) response content.

### Request Example
```js
{
    "name": "Rhino Automation - Chrome & Firefox",
    "testsRepository": [ ],
    "driverParameters": [
        {
            "driver": "ChromeDriver",
            "driverBinaries": "http://localhost:4444/wd/hub"
        },
        {
            "driver": "FirefoxDriver",
            "driverBinaries": "http://localhost:4444/wd/hub"
        }
    ],
    "dataSource": [],
    "models": [ ],
    "connector": "connector_text",
    "gravityEndpoint": "",
    "authentication": {
        "password": "<rhino_user>",
        "userName": "<rhino_password>"
    },
    "engineConfiguration": {
        "maxParallel": 1,
        "failOnException": false,
        "optimalThreshold": 3.0,
        "qualityThreshold": 0.0,
        "toleranceThreshold": 0.0,
        "priority": 0,
        "severity": 0,
        "errorOnExitCode": 0,
        "elementSearchingTimeout": 15000,
        "pageLoadTimeout": 60000,
        "retrunExceptions": true,
        "returnPerformancePoints": true,
        "returnEnvironment": true,
        "terminateOnAssertFailure": false
    },
    "screenshotsConfiguration": {
        "keepOriginal": false,
        "returnScreenshots": false,
        "screenshotsOut": "<path_to_screenshots_folder>",
        "onExceptionOnly": false
    },
    "reportConfiguration": {
        "reportOut": "<path_to_reports_folder>",
        "logsOut": "<path_to_logs_folder>",
        "reporters": null,
        "connectionString": null,
        "dataProvider": null,
        "archive": false,
        "localReport": true,
        "addGravityData": true
    }
}
```

### Response Codes
|Code|Description                                                                                  |
|----|---------------------------------------------------------------------------------------------|
|201 |Success, the _**Configuration**_ created and identifier was returned as part of the response.|
|400 |Bad Request, the request is missing a mandatory field(s) or bad formatted.                   |
|500 |Fail, the server encountered an unexpected error.                                            |

## Update Configuration
Updates an existing _**Rhino Configuration**_.

```
PUT /api/v3/configurations/:configuration_id
```

|Name            |Type  |Description                             |
|----------------|------|----------------------------------------|
|configuration_id|string|The ID of the _**Rhino Configuration**_.|

### Request Fields
The request body follows the same format as [Get Configuration](#get-configuration) [Response Content](#response-content).

### Request Example
```js
{
    "name": "Rhino Automation - Chrome & Firefox - After Update",
    "testsRepository": [ ],
    "driverParameters": [
        {
            "driver": "ChromeDriver",
            "driverBinaries": "http://localhost:4444/wd/hub"
        },
        {
            "driver": "FirefoxDriver",
            "driverBinaries": "http://localhost:4444/wd/hub"
        }
    ],
    "dataSource": [],
    "models": [ ],
    "connector": "connector_text",
    "gravityEndpoint": "",
    "authentication": {
        "password": "<rhino_user>",
        "userName": "<rhino_password>"
    },
    "engineConfiguration": {
        "maxParallel": 1,
        "failOnException": false,
        "optimalThreshold": 3.0,
        "qualityThreshold": 0.0,
        "toleranceThreshold": 0.0,
        "priority": 0,
        "severity": 0,
        "errorOnExitCode": 0,
        "elementSearchingTimeout": 15000,
        "pageLoadTimeout": 60000,
        "retrunExceptions": true,
        "returnPerformancePoints": true,
        "returnEnvironment": true,
        "terminateOnAssertFailure": false
    },
    "screenshotsConfiguration": {
        "keepOriginal": false,
        "returnScreenshots": false,
        "screenshotsOut": "<path_to_screenshots_folder>",
        "onExceptionOnly": false
    },
    "reportConfiguration": {
        "reportOut": "<path_to_reports_folder>",
        "logsOut": "<path_to_logs_folder>",
        "reporters": null,
        "connectionString": null,
        "dataProvider": null,
        "archive": false,
        "localReport": true,
        "addGravityData": true
    }
}
```

### Response Codes
|Code|Description                                                                          |
|----|-------------------------------------------------------------------------------------|
|200 |Success, the _**Configuration**_ was returned as part of the response.               |
|400 |Bad Request, the request is missing a mandatory field(s) or bad formatted.           |
|404 |Not Found, the _**Configuration**_ was not found under the configurations collection.|
|500 |Fail, the server encountered an unexpected error.                                    |

## Delete Configuration
Deletes an existing _**Rhino Configuration**_.

```
DELETE /api/v3/configurations/:configuration_id
```

|Name            |Type  |Description                             |
|----------------|------|----------------------------------------|
|configuration_id|string|The ID of the _**Rhino Configuration**_.|

> Please Note: Deleting a configuration cannot be undone. It does not, however, affect test cases/models.

### Response Codes
|Code|Description                                                                          |
|----|-------------------------------------------------------------------------------------|
|204 |Success, the _**Configuration**_ was deleted.                                        |
|404 |Not Found, the _**Configuration**_ was not found under the configurations collection.|
|500 |Fail, the server encountered an unexpected error.                                    |

## Delete Configurations
Deletes all existing _**Rhino Configuration**_ for the authenticated user.

```
DELETE /api/v3/configurations
```

> Please Note: Deleting a configuration cannot be undone. It does not, however, affect test cases/models.

### Response Codes
|Code|Description                                      |
|----|-------------------------------------------------|
|204 |Success, the _**Configurations**_ were deleted.  |
|500 |Fail, the server encountered an unexpected error.|