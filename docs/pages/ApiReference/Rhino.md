[Home](../Home.md 'Home')  

# API - Rhino
02/16/2021 - 75 minutes to read

## In This Article
* [Invoke Configuration](#invoke-configuration)
* [Invoke Configuration by ID](#invoke-configuration-by-id)
* [Invoke Collection by Configuration ID](#invoke-collection-by-configuration-id)
* [Invoke Collection by Collection ID and Configuration ID](#invoke-collection-by-collection-id-and-configuration-id)
* [Invoke Collection by Collection ID](#invoke-collection-by-collection-id)

Use the following API methods to invoke (run) operations like [configurations](./Configurations.md) or [tests collections](./TestCases) against Rhino API.

## Invoke Configuration
Invokes (run) a _**Rhino Configuration**_.

```
POST /rhino/configurations/invoke
```

### Request Fields
The request body follows the same format as [Get Configuration](./Configurations.md#get-configuration) response content.

### Request Example
```js
{
  "engineConfiguration": {
    "errorOnExitCode": 10
  },
  "authentication": {
    "userName": "rhino_user",
    "password": "rhino_password"
  },
  "driverParameters": [
    {
      "driver": "ChromeDriver",
      "driverBinaries": "http://localhost:4444/wd.hub"
    }
  ],
  "name": "Execute Tests by Raw Text",
  "testsRepository": [
    "[test-id] Text-000\r\n[test-scenario]\r\nOpen a Web Site\r\n[test-actions]\r\n1. go to url {https://gravitymvctestapplication.azurewebsites.net/}\r\n3. wait {3000}\r\n6. close browser"
  ]
}
```
### Response Content
```js
{
  "key": "unattached-2020.08.19.06.36.43.698",
  "actual": true,
  "reasonPhrase": "",
  "title": "Rhino Automation - Test Run Generator (ID: 2020.08.19.06.36.43.698); (Configuration: Rhino Automation - Chrome)",
  "start": "2020-08-19T18:36:43.6989032+03:00",
  "end": "2020-08-19T18:37:36.4887447+03:00",
  "runTime": "00:00:52.7898415",
  "testCases": [
    {
      "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0",
      "key": "rhino-documentation-001",
      "testSuite": "",
      "testRunKey": "unattached-2020.08.19.06.36.43.698",
      "scenario": "search students on https://gravitymvctestapplication.azurewebsites.net/student web-site",
      "reasonPhrase": "",
      "actual": true,
      "steps": [
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-0",
          "testCase": "",
          "action": "1. go to url {https://gravitymvctestapplication.azurewebsites.net/student} using any compliant web-browser",
          "command": "GoToUrl",
          "expected": "<span>assert {url} match {gravitymvctestapplication.azurewebsites.net}</span>",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:14.2072016"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-1",
          "testCase": "",
          "action": "2. close all child windows (to make sure only the web site is open and visible)",
          "command": "CloseAllChildWindows",
          "expected": "",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:00.0119308"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-2",
          "testCase": "",
          "action": "3. send keys {Carson} into {//input[@id='SearchString']}",
          "command": "SendKeys",
          "expected": "",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:00.1368129"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-3",
          "testCase": "",
          "action": "4. click on {#SearchButton} using {css selector}",
          "command": "Click",
          "expected": "",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:00.2752300"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-4",
          "testCase": "",
          "action": "5. wait for {3000} milliseconds",
          "command": "Wait",
          "expected": "<span>assert {count} on {//td[@id]} is greater than {0}</span>",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:03.0361094"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-5",
          "testCase": "",
          "action": "6. register parameter {first_name} take {Jhon}",
          "command": "RegisterParameter",
          "expected": "",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:15.0670764"
        },
        {
          "identifier": "unattached-2020.08.19.06.36.43.698-rhino-documentation-001-0-0-6",
          "testCase": "",
          "action": "7. close browser",
          "command": "CloseBrowser",
          "expected": "",
          "actual": true,
          "reasonPhrase": "",
          "link": "",
          "runTime": "00:00:00.1242385"
        }
      ],
      "totalSteps": 7,
      "link": "",
      "iteration": 0,
      "dataSource": [
        {
          "address": "https://gravitymvctestapplication.azurewebsites.net/student",
          "address-expected": "gravitymvctestapplication.azurewebsites.net",
          "search-text-box": "//input[@id='SearchString']",
          "search-button": "#SearchButton",
          "students-table": "//td[@id]"
        }
      ],
      "modelEntries": [],
      "priority": "",
      "severity": "",
      "tolerance": 0.0,
      "passedOnAttempt": 0,
      "qualityRank": 100.0,
      "inconclusive": false,
      "start": "2020-08-19T18:36:43.9059141+03:00",
      "end": "2020-08-19T18:37:36.4732305+03:00",
      "runTime": "00:00:52.5673164",
      "environment": {
        "applicationParams": {},
        "macroParams": {},
        "sessionParams": {
          "first_name": "Jhon"
        }
      }
    }
  ],
  "totalTests": 1,
  "totalSteps": 7,
  "totalPass": 1,
  "totalPassSteps": 7,
  "totalFail": 0,
  "totalFailSteps": 0,
  "totalIterations": 1,
  "totalInconclusive": 0,
  "successRate": 100.0,
  "qualityRank": 100.0,
  "link": null,
  "performancePoints": {
    "rhino-documentation-001_0": 0.87612194
  },
  "priorityPoints": {
    "": 0
  },
  "severityPoints": {
    "": 0
  },
  "aboveOptimalRate": 0.0,
  "aboveOptimalCount": 0,
  "belowOptimalRate": 100.0,
  "belowOptimalCount": 1,
  "averageTestTime": "00:00:52.5673164",
  "totalTimeouts": "00:00:00",
  "loadTimeouts": "00:00:00",
  "elementTimeouts": "00:00:00",
  "severity": 0,
  "priority": 0,
  "tolerance": 0.0
}
```

The following system fields are always included in the response:

#### General
|Name                    |Type     |Description                                                                                   |
|------------------------|---------|----------------------------------------------------------------------------------------------|
|key                     |string   |The unique identifier of this _**Rhino Test Run**_.                                           |
|actual                  |boolean  |The actual result of this _**Rhino Test Run**_. ```true``` for pass, ```false``` for fail.    |
|reasonPhrase            |string   |The reason of why this _**Rhino Test Run**_ failed.                                           |
|title                   |string   |The title of this _**Rhino Test Run**_.                                                       |
|start                   |date+time|The start time of this _**Rhino Test Run**_.                                                  |
|end                     |date+time|The start time of this _**Rhino Test Run**_.                                                  |
|runTime                 |time     |The run time (total) of this _**Rhino Test Run**_.                                            |
|[testCases](#test-cases)|array    |A collection of _**Rhino Test Case**_ executed under this  _**Rhino Test Run**_.              |
|totalSteps              |number   |The total steps number of this _**Rhino Test Case**_.                                         |
|totalIterations         |number   |The total iterations number of this _**Rhino Test Run**_.                                     |
|totalInconclusive       |number   |The total inconclusive tests number of this _**Rhino Test Run**_.                             |
|successRate             |double   |The success rate of this _**Rhino Test Run**_.                                                |
|qualityRank             |double   |The quality rank of this _**Rhino Test Run**_.                                                |
|link                    |string   |The link pointing to this _**Rhino Test Run**_ if you are using any ALM connector.            |
|performancePoints       |object   |The performance points of this _**Rhino Test Run**_, aggregated test iteration execution time.|
|priorityPoints          |object   |The priority points of this _**Rhino Test Run**_, priority rank per Rhino Test Case.          |
|severityPoints          |object   |The severity points of this _**Rhino Test Run**_, severity rank per Rhino Test Case.          |
|aboveOptimalRate        |double   |The rate of tests which their running time is above the optimal running time threshold.       |
|aboveOptimalCount       |number   |The number of tests which their running time is above the optimal running time threshold.     |
|belowOptimalRate        |double   |The rate of tests which their running time is below the optimal running time threshold.       |
|belowOptimalCount       |number   |The number of tests which their running time is below the optimal running time threshold.     |
|totalTests              |number   |The total tests number of this _**Rhino Test Run**_.                                          |
|totalSteps              |number   |The total steps number of this _**Rhino Test Run**_.                                          |
|totalPass               |number   |The total passed tests number of this _**Rhino Test Run**_.                                   |
|averageTestTime         |double   |The average individual test time of this _**Rhino Test Run**_.                                |
|totalTimeouts           |time     |The total time spent on timeouts for this _**Rhino Test Run**_.                               |
|loadTimeouts            |time     |The total time spent on page load timeouts for this _**Rhino Test Run**_.                     |
|elementTimeouts         |time     |The total time spent on elements search timeouts for this _**Rhino Test Run**_.               |
|severity                |number   |The severity level of this _**Rhino Test Run**_.                                              |
|priority                |number   |The priority level of this _**Rhino Test Run**_.                                              |
|tolerance               |double   |The tolerance level of this _**Rhino Test Run**_.                                             |

#### Test Cases
|Name                       |Type     |Description                                                                                                                    |
|---------------------------|---------|-------------------------------------------------------------------------------------------------------------------------------|
|identifier                 |string   |The unique identifier of this _**Rhino Test Case*_.                                                                            |
|key                        |string   |The _**Rhino Test Case*_ ID.                                                                                                   |
|testSuite                  |string   |The ID of the test suite which test belongs to.                                                                                |
|testRunKey                 |string   |The unique identifier of the test run which runs under.                                                                        |
|scenario                   |string   |The title this _**Rhino Test Case*_.                                                                                           |
|reasonPhrase               |string   |The reason of why this _**Rhino Test Case**_ failed.                                                                           |
|actual                     |boolean  |The actual result of this _**Rhino Test Case**_. ```true``` for pass, ```false``` for fail.                                    |
|[steps](#steps)            |array    |A collection of _**Rhino Test Step**_ executed under this  _**Rhino Test Case**_.                                              |
|totalSteps                 |number   |The total steps number of this _**Rhino Test Case**_.                                                                          |
|iteration                  |number   |The iteration number of this _**Rhino Test Case**_. Iterations are created when test run on data source or on multiple drivers.|
|dataSource                 |object   |The local data source (iteration will be created for each data row) of this _**Rhino Test Case**_.                             |
|modelEntries               |object   |A collection of _**Rhino Model**_ used by this _**Rhino Test Case**_.                                                          |
|priority                   |number   |The priority level of this _**Rhino Test Case**_.                                                                              |
|severity                   |number   |The severity level of this _**Rhino Test Case**_.                                                                              |
|tolerance                  |double   |The tolerance level of this _**Rhino Test Case**_.                                                                             |
|passedOnAttempt            |number   |On which attempt (when retry) the test has passed. The value 0 will be given if the test failed.                               |
|qualityRank                |double   |The quality rank of this _**Rhino Test Case**_.                                                                                |
|inconclusive               |Boolean  |If set to true, this test will be marked with warning when fails.                                                              |
|start                      |date+time|The start time of this _**Rhino Test Case**_.                                                                                  |
|end                        |date+time|The start time of this _**Rhino Test Case**_.                                                                                  |
|runTime                    |time     |The run time (total) of this _**Rhino Test Case**_.                                                                            |
|[environment](#environment)|object   |The automation environment data state (parameters current value) from Rhino Server State.                                      |

#### Steps
|Name        |Type   |Description                                                                                |
|------------|-------|-------------------------------------------------------------------------------------------|
|identifier  |string |The unique identifier of this _**Rhino Test Step*_.                                        |
|testCase    |string |The parent test case identifier.                                                           |
|action      |string |The test action (i.e. go to URL {https://www.foo.io}).                                     |
|command     |string |The command used for this action (plugin command).                                         |
|expected    |string |The expected result of this _**Rhino Test Step*_.                                          |
|actual      |boolean|The actual result of this _**Rhino Test Case**_. ```true``` for pass, ```false``` for fail.|
|reasonPhrase|string |The reason of why this _**Rhino Test Case**_ failed.                                       |
|link        |string |The link pointing to this _**Rhino Test Step**_ if you are using any ALM connector.        |
|runTime     |time   |The run time (total) of this _**Rhino Test Step**_.                                        |

#### Environment
|Name             |Type  |Description                                                                                                                  |
|-----------------|------|-----------------------------------------------------------------------------------------------------------------------------|
|applicationParams|object|The application parameters - Gravity Environment parameters, available for all runs. Will only reset when restart the server.|
|sessionParams    |object|The session parameters - Gravity Environment parameters, available for a single run.  Will reset when run is completed.      |
|macorParams      |object|The macro parameters - Gravity Macro parameters, available for a single run.  Will reset when run is completed.              |

### Response Codes
|Code|Description                                                                |
|----|---------------------------------------------------------------------------|
|200 |Success, the _**Rhino Test Run**_ is returned as part of the response.     |
|400 |Bad Request, _**Rhino Configuration**_ was not provided or badly formatted.|
|500 |Fail, the server encountered an unexpected error.                          |

## Invoke Configuration by ID
Invokes (run) a _**Rhino Configuration**_.

```
GET /rhino/configurations/invoke/:id
```

|Name|Type  |Description                                       |
|----|------|--------------------------------------------------|
|id  |string|The ID of the _**Rhino Configuration**_ to invoke.|

### Response Content
The response body follows the same format as [Invoke Configuration](#invoke-configuration) response content.

### Response Codes
|Code|Description                                                                |
|----|---------------------------------------------------------------------------|
|200 |Success, the _**Rhino Test Run**_ is returned as part of the response.     |
|400 |Bad Request, _**Rhino Configuration**_ was not provided or badly formatted.|
|404 |Not Found, _**Rhino Configuration**_ was not found by the provided id.     |
|500 |Fail, the server encountered an unexpected error.                          |

## Invoke Collection by Configuration ID
Invokes (run) _**Rhino Spec**_ directly from the request body.

```
POST /rhino/configurations/:configuration_id/collections/invoke
```

|Name            |Type  |Description                                                                     |
|----------------|------|--------------------------------------------------------------------------------|
|configuration_id|string|The ID of the _**Rhino Configuration**_ to use when invoke the _**Rhino Spec**_.|

### Request Fields
The request body follows the same format as _**Get Test Case Collection**_ response content.

### Request Example
Please see below for a typical request:

```
[test-id] rhino-documentation-001
[test-scenario] search students on https://gravitymvctestapplication.azurewebsites.net/student web-site

[test-actions]
1. go to url {@address} using any compliant web-browser
2. close all child windows (to make sure only the web site is open and visible)
3. send keys {Carson} into {@search-text-box} text-box
4. click on {@search-button} using {css selector}
5. wait for {3000} milliseconds
6. close browser

[test-expected-results]
[1] assert {url} match {@address-expected}
[5] assert {count} on {@search-text-box} is greater than {0}

[test-data-provider]
[
	{
		"address":"https://gravitymvctestapplication.azurewebsites.net/student",
		"address-expected":"gravitymvctestapplication.azurewebsites.net",
		"search-text-box":"//input[@id='SearchString']",
		"search-button":"#SearchButton",
		"students-table": "//td[@id]"
	}
]

>>>

[test-id] rhino-documentation-002
...
```

### Response Content

The response body follows the same format as [Run by Configuration](#run-by-configuration) response content.

### Response Codes
|Code|Description                                                         |
|----|--------------------------------------------------------------------|
|200 |Success, the _**Rhino Result**_ is returned as part of the response.|
|400 |Bad Request, _**Rhino Configuration**_ was not provided.            |
|404 |Not Found, the _**Rhino Configuration**_ was not found.             |
|500 |Fail, the server encountered an unexpected error.                   |

## Invoke Collection by Collection ID and Configuration ID
Invokes (run) _**Rhino Spec**_ directly from pre-existing collection & pre-existing configuration.

```
POST /rhino/configurations/:configuration_id/collections/invoke/:collection_id
```

|Name            |Type  |Description                                                                     |
|----------------|------|--------------------------------------------------------------------------------|
|collection_id   |string|The ID of the _**Rhino Collection**_ to use.                                    |
|configuration_id|string|The ID of the _**Rhino Configuration**_ to use when invoke the _**Rhino Spec**_.|

### Response Content

The response body follows the same format as [Run by Configuration](#run-by-configuration) response content.

### Response Codes
|Code|Description                                                                        |
|----|-----------------------------------------------------------------------------------|
|200 |Success, the _**Rhino Result**_ is returned as part of the response.               |
|400 |Bad Request, _**Rhino Configuration**_ or _**Rhino Collection**_ were not provided.|
|404 |Not Found, the _**Rhino Configuration**_  or _**Rhino Collection**_ were not found.|
|500 |Fail, the server encountered an unexpected error.                                  |

## Invoke Collection by Collection ID
Invokes (run) _**Rhino Spec**_ directly from pre-existing collection for all configuration assigned to the selected collection.

```
POST /rhino/collections/invoke/:id
```

|Name|Type  |Description                                    |
|----|------|-----------------------------------------------|
|id  |string|The ID of the _**Rhino Collection**_ to invoke.|

### Response Content

The response body follows the same format as [Run by Configuration](#run-by-configuration) response content.

### Response Codes
|Code|Description                                                         |
|----|--------------------------------------------------------------------|
|200 |Success, the _**Rhino Result**_ is returned as part of the response.|
|400 |Bad Request, the **Rhino Collection**_ were not provided.           |
|404 |Not Found, the _**Rhino Collection**_ were not found.               |
|500 |Fail, the server encountered an unexpected error.                   |