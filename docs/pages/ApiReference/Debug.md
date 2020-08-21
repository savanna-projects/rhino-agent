# API: Debug
Use the following API methods to simulate a debugging process of your automation and gets underline exceptions and extractions.

> Note, the API used for these requests is the underline [Gravity API](https://github.com/gravity-api?tab=repositories).

## Execute Gravity API Request
Returns an _**Orbit Response**_ object.

```
GET /api/v3/debug
```

### Request Content
Please see below for a typical request:

```js
{
  "DataSource": null,
  "Authentication": {
    "Password": "rhinoPassword",
    "UserName": "rhinoUserName"
  },
  "EngineConfiguration": {
    "MaxParallel": 1,
    "FailOnException": false,
    "OptimalThreshold": 3.0,
    "QualityThreshold": 0.0,
    "ToleranceThreshold": 0.0,
    "Priority": 0,
    "Severity": 0,
    "ErrorOnExitCode": 0,
    "ElementSearchingTimeout": 15000,
    "PageLoadTimeout": 60000,
    "RetrunExceptions": true,
    "ReturnPerformancePoints": true,
    "ReturnEnvironment": true,
    "TerminateOnAssertFailure": false
  },
  "ScreenshotsConfiguration": {
    "KeepOriginal": false,
    "ReturnScreenshots": true,
    "ScreenshotsOut": "D:\\sites\\RhinoOutputs\\Images"
  },
  "DriverParams": "{\r\n  \"driver\": \"ChromeDriver\",\r\n  \"driverBinaries\": \"D:\\\\automation-env\\\\web-drivers\"\r\n}",
  "Extractions": null,
  "Actions": [
    {
      "ActionType": "GoToUrl",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "",
      "RegularExpression": ".*",
      "Argument": "https://gravitymvctestapplication.azurewebsites.net/student"
    },
    {
      "ActionType": "Assert",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "",
      "RegularExpression": ".*",
      "Argument": "{{$ --url --match:gravitymvctestapplication.azurewebsites.net}}"
    },
    {
      "ActionType": "CloseAllChildWindows",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "",
      "RegularExpression": ".*",
      "Argument": ""
    },
    {
      "ActionType": "SendKeys",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "//input[@id='SearchString']",
      "RegularExpression": ".*",
      "Argument": "Carson"
    },
    {
      "ActionType": "Click",
      "Locator": "CssSelector",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "#SearchButton",
      "RegularExpression": ".*",
      "Argument": ""
    },
    {
      "ActionType": "Wait",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "",
      "RegularExpression": ".*",
      "Argument": "3000"
    },
    {
      "ActionType": "Assert",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "//td[@id]",
      "RegularExpression": ".*",
      "Argument": "{{$ --count --gt:0}}"
    },
    {
      "ActionType": "RegisterParameter",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "Jhon",
      "RegularExpression": ".*",
      "Argument": "first_name"
    },
    {
      "ActionType": "CloseBrowser",
      "Locator": "Xpath",
      "Reference": 0,
      "RepeatReference": 0,
      "Actions": [],
      "ElementAttributeToActOn": "",
      "ElementToActOn": "",
      "RegularExpression": ".*",
      "Argument": ""
    }
  ]
}
```

The following system fields must be always included in the request:

#### General
|Name                                                  |Type  |Description                                                                  |
|------------------------------------------------- ----|------|-----------------------------------------------------------------------------|
|[authentication](#authentication)                     |object|A collection of extraction objects returned by Gravity API.                  |
|[engineConfiguration](#engine-configuration)          |object|A set of data, based on the request sent to Gravity API.                     |
|[screenshotsConfiguration](#screenshots-configuration)|object|A set of data, based on the request sent to Gravity API.                     |
|driverParams                                          |string|Parameters which represents the target platforms on which the tests will run.|
|[actions](#gravity-action)                            |array |A collection of _**Gravity Plugin**_ to execute by this request.             |

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

#### Screenshots Configuration
|Name             |Type   |Description                                                                                               |
|-----------------|-------|----------------------------------------------------------------------------------------------------------|
|keepOriginal     |boolean|When set to true, will keep the original file created by Gravity engine, when creating a new Rhino report.|
|returnScreenshots|boolean|When set to false, screenshots will be returned from Gravity engine.                                      |
|screenshotsOut   |decimal|The directory in which to save automatic screenshots.                                                     |

#### Gravity Action
|Name                   |Type  |Description                                                                                                                                              |
|-----------------------|------|---------------------------------------------------------------------------------------------------------------------------------------------------------|
|actionType             |string|_**Gravity Plugin**_ name (i.e. Click or SendKeys).                                                                                                      |
|Locator                |string|Elements locator type (i.e. Xpath or CssSelector).                                                                                                       |
|Reference              |number|A zero based index of the _**Gravity Plugin**_ in the _**Web Automation**_ actions array (read only property).                                           |
|RepeatReference        |number|A zero based index of the _**Gravity Plugin**_ n the _**Repeat**_ plugin actions array (read only property).                                             |
|ElementAttributeToActOn|string|The element attribute name on which this _**Gravity Plugin**_ will act (i.e. href or class).                                                             |
|ElementToActOn         |string|The element on which this _**Gravity Plugin**_ will act. This will be the locator value (i.e. if the locator type is CssSelector, it will be #myElement).|
|RegularExpression      |string|A regular expression to apply on attribute or inner text values of an element, before the _**Gravity Plugin**_ is executed.                              |
|Argument               |string|An argument to pass along with this _**Gravity Plugin**_ (i.e. if the action is SendKeys the argument can be "hello world!".                             |

### Response Content
Please see below for a typical response:

```js
{
  "extractions": [
    {
      "key": "1",
      "entities": [
        {
          "entityContentEntries": {
            "actual": "3",
            "expected": "0",
            "method": "gt",
            "assertion": "true"
          }
        }
      ],
      "orbitSession": {
        "sessionsId": "2a0d998832ff9bd2f859a16a9664cb37",
        "machineName": "DESKTOP-G1MC8H7",
        "machineIp": "192.168.1.21"
      }
    },
    {
      "key": "0",
      "entities": [
        {
          "entityContentEntries": {
            "actual": "https://gravitymvctestapplication.azurewebsites.net/student",
            "expected": "gravitymvctestapplication.azurewebsites.net",
            "method": "match",
            "assertion": "true"
          }
        }
      ],
      "orbitSession": {
        "sessionsId": "2a0d998832ff9bd2f859a16a9664cb37",
        "machineName": "DESKTOP-G1MC8H7",
        "machineIp": "192.168.1.21"
      }
    }
  ],
  "orbitRequest": {
    "serializedRequest": null,
    "serializedResponse": null,
    "exceptions": [],
    "performancePoints": [
      {
        "time": 3659.9477,
        "actionReference": 0,
        "action": "GoToUrl",
        "repeatReference": 0
      },
      {
        "time": 383.979,
        "actionReference": 1,
        "action": "Assert",
        "repeatReference": 0
      },
      {
        "time": 352.1497,
        "actionReference": 2,
        "action": "CloseAllChildWindows",
        "repeatReference": 0
      },
      {
        "time": 469.0115,
        "actionReference": 3,
        "action": "SendKeys",
        "repeatReference": 0
      },
      {
        "time": 648.0932,
        "actionReference": 4,
        "action": "Click",
        "repeatReference": 0
      },
      {
        "time": 3341.0645,
        "actionReference": 5,
        "action": "Wait",
        "repeatReference": 0
      },
      {
        "time": 372.835,
        "actionReference": 6,
        "action": "Assert",
        "repeatReference": 0
      },
      {
        "time": 15435.1959,
        "actionReference": 7,
        "action": "RegisterParameter",
        "repeatReference": 0
      },
      {
        "time": 137.4526,
        "actionReference": 8,
        "action": "CloseBrowser",
        "repeatReference": 0
      }
    ],
    "screenshots": [
      {
        "actionReference": 7,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090121559-7-RegisterParameter.png"
      },
      {
        "actionReference": 6,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090106121-6-Assert.png"
      },
      {
        "actionReference": 5,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090105752-5-Wait.png"
      },
      {
        "actionReference": 4,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090102338-4-Click.png"
      },
      {
        "actionReference": 3,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090101706-3-SendKeys.png"
      },
      {
        "actionReference": 2,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090101249-2-CloseAllChildWindows.png"
      },
      {
        "actionReference": 1,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090100860-1-Assert.png"
      },
      {
        "actionReference": 0,
        "comment": "info",
        "type": "PNG",
        "location": "D:\\sites\\RhinoOutputs\\Images\\20200821090100416-0-GoToUrl.png"
      }
    ],
    "userName": "automation@rhino.api",
    "startTime": "2020-08-21T12:00:52.0187856+03:00",
    "endTime": "2020-08-21T12:01:22.0720038+03:00",
    "totalRunTime": 30053,
    "responseSize": 5550,
    "requestSize": 4794,
    "environment": {
      "applicationParams": {},
      "macroParams": {},
      "sessionParams": {
        "first_name": "John"
      }
    }
  }
}
```

The following system fields are always included in the response:

#### General
|Name                         |Type  |Description                                                |
|-----------------------------|------|-----------------------------------------------------------|
|[extractions](#extraction)   |array |A collection of extraction objects returned by Gravity API.|
|[orbitRequest](#Orbit-request)|object|A set of data, based on the request sent to Gravity API.  |

#### Extraction
|Name                          |Type  |Description                                                                  |
|------------------------------|------|-----------------------------------------------------------------------------|
|key                           |string|The unique identifier for this extraction entry.                             |
|[entities](#entity)           |array |A collection of information which describes an entity (as map or dictionary).|
|[orbitSession](#orbit-session)|object|Gravity API session information.                                             |

#### Entity
|Name   |Type  |Description                                                |
|-------|------|-----------------------------------------------------------|
|content|object|A collection of Key/Value which describes an entity schema.|

#### Orbit Session
|Name       |Type   |Description                                                                |
|-----------|-------|---------------------------------------------------------------------------|
|sessionsId |string|Gravity API session ID. Will be the WebDriver session if WebDriver was used.|
|machineName|string|The machine name under which this Gravity API session was executed.         |
|machineIp  |string|The machine IP address under which this Gravity API session was executed.   |

#### Orbit Request
|Name                                   |Type     |Description                                                                                                       |
|---------------------------------------|---------|------------------------------------------------------------------------------------------------------------------|
|serializedRequest                      |string   |The serialized _**Web Automation**_ request sent by the client. _Will always be null due do privacy policies_.    |
|serializedResponse                     |string   |The serialized _**Orbit Response**_ object returned by Gravity API. _Will always be null due do privacy policies_.|
|[exceptions](#orbit-exception)         |array    |A collection of _**Orbit Exception**_ object thrown during execution.                                             |
|[performancePoints](#performance-point)|array    |A collection of _**Orbit Performance Point**_ object.                                                             |
|[screenshots](#gravity-screenshot)     |array    |A collection of _**Orbit Screenshot**_ object.                                                                    |
|userName                               |string   |The user name used to execute this _**Web Automation**_ request.                                                  |
|start                                  |date+time|The start time of this _**Web Automation**_.                                                                      |
|end                                    |date+time|The end time of this _**Web Automation**_.                                                                        |
|runTime                                |time     |The run time (total) of this _**Web Automation**_.                                                                |
|responseSize                           |number   |Response size in KB of the _**Orbit Response**_ object returned by Gravity API.                                   |
|requestSize                            |number   |Response size in KB of the _**Web Automation**_ object sent to Gravity API.                                       |
|environment                            |object   |The run time (total) of this _**Web Automation**_.                                                                |
|[environment](#environment)            |object   |The automation environment data state (parameters current value) from Rhino Server State.                         |

#### Orbit Exception
|Name           |Type  |Description                                                                                                        |
|---------------|------|-------------------------------------------------------------------------------------------------------------------|
|exception      |object|The thrown exception full stack information.                                                                       |
|actionReference|number|A zero based index of the _**Gravity Plugin**_ which throw the exception in the _**Web Automation**_ actions array.|
|action         |string|The _**Gravity Plugin**_ which throw the exception (i.e. Click or SendKeys).                                       |
|screenshot     |string|The full path of this exception screenshot (if taken).                                                             |
|repeatReference|number|A zero based index of the _**Gravity Plugin**_ which throw the exception in the _**Repeat**_ plugin actions array. |
|context        |object|A context for this exception which can hold an extra information.                                                  |

#### Performance Point
|Name           |Type  |Description                                                                              |
|---------------|------|-----------------------------------------------------------------------------------------|
|time           |double|The total run time of the _**Gravity Plugin**_ (i.e. Click or SendKeys).                 |
|actionReference|number|A zero based index of the _**Gravity Plugin**_ in the _**Web Automation**_ actions array.|
|action         |string|The _**Gravity Plugin**_ which throw the exception (i.e. Click or SendKeys).             |
|repeatReference|number|A zero based index of the _**Gravity Plugin**_ in the _**Repeat**_ plugin actions array. |

#### Screenshot
|Name           |Type  |Description                                                                              |
|---------------|------|-----------------------------------------------------------------------------------------|
|actionReference|number|A zero based index of the _**Gravity Plugin**_ in the _**Web Automation**_ actions array.|
|comment        |string|Any text for describing the screenshot.                                                  |
|type           |string|The image file type (i.e. PNG or JPG).                                                   |
|location       |string|The full path of this exception screenshot (if taken).                                   |

#### Environment
|Name             |Type  |Description                                                                                                                  |
|-----------------|------|-----------------------------------------------------------------------------------------------------------------------------|
|applicationParams|object|The application parameters - Gravity Environment parameters, available for all runs. Will only reset when restart the server.|
|applicationParams|object|The session parameters - Gravity Environment parameters, available for a single runs.  Will reset when run is completed.     |
|macorParams      |object|The macro parameters - Gravity Macro parameters, available for a single runs.  Will reset when run is completed.             |

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Orbit Response**_ was returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                      |