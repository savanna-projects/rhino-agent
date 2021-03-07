[Home](../Home.md 'Home')  

# API: Logs
02/16/2021 - 35 minutes to read

## In This Article
* [Get Logs Files](#get-logs-files)
* [Get Log](#get-log)
* [Get Log by Number of Lines](#get-log-by-number-of-lines)
* [Export Log](#export-log)

_**Rhino Logger**_ logs all information, exceptions and executions details in a designated log. It is possible to access the logs directly from the location where Rhino Server saves them, but in some cases where it is not possible to access directly to the server or if the logs were not stored under a shared location, it is also possible to access and read the logs using this API.

> _**Information**_
>  
> 1. Each day a new log will be created automatically. The log name is the date where is was created in the following format ```RhinoApi-yyyyMMdd.log```.
> 2. It is possible to control the logs location in the [Report Configuration](./Configurations.md#report-configuration).  

Use the following API methods to request details from _**Rhino Logs**_.

## Get Logs Files
Returns an existing _**Automation Logs**_ files list.

```
GET /api/v3/logs
```

#### Response Content

```js
[
  "RhinoApi-20201230.log",
  "RhinoApi-20201231.log",
  ...
]
```

### Response Codes
|Code|Description                                                         |
|----|--------------------------------------------------------------------|
|200 |Success, the _**Automation Logs**_ returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                   |

## Get Log
Returns an existing _**Automation Log**_.

> Configuration key to set logs folder is ```reportConfiguration.logsOut```.
> If not specified, the default logs directory is ```<current_directory>\Logs```.

```
GET /api/v3/logs/:log_id
```

|Name            |Type  |Description                                                                                |
|----------------|------|-------------------------------------------------------------------------------------------|
|log_id          |string|The ID of the _**Automation Log**_. Will be the date in the following format ```yyyyMMdd```|

#### Response Content
> The response is an array of log entries of media type ```text/plain```.
> Log entries are separated by an empty line.

```
DBG - 2020-07-08 07:31:29.092
    Application: kdd.engine
    Logger     : kdd.engine.text-connector
    LogLevel   : DEBUG
    TimeStamp  : 2020-07-08 07:31:29.092
    MachineName: DESKTOP-G1MC8H7
    Message    : [OnBeforeTestExecution] does not need an implementation for this connector

Automation.Kdd.Agent Information: 0 : INF - 2020-07-08 07:31:29.094
    Application: kdd.engine
    Logger     : kdd.engine.text-connector
    LogLevel   : INFO
    TimeStamp  : 2020-07-08 07:31:29.094
    MachineName: DESKTOP-G1MC8H7
    Message    : executing 'Login'

Automation.Kdd.Agent Information: 0 : [GoToUrl]; argument [https://gravitymvctestapplication.azurewebsites.net/] executed
Automation.Kdd.Agent Information: 0 : [SendKeys]; element [UserName]; argument [userName] executed
Automation.Kdd.Agent Information: 0 : [SendKeys]; element [Password]; argument [password] executed
Automation.Kdd.Agent Information: 0 : [Click]; element [//button[contains(.,'Log In')]] executed
Automation.Kdd.Agent Information: 0 : [WaitForUrl] executed
Automation.Kdd.Agent Information: 0 : [Assert]; argument [{{$ --url --match:Dashboard}}] executed
Automation.Kdd.Agent Information: 0 : [CloseBrowser] executed
Automation.Kdd.Agent Information: 0 : [web-automation]; argument [1/1] executed
...
```

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Automation Logs**_ is returned as part of the response.|
|404 |Not Found, the _**Automation Logs**_ were not found.                   |
|500 |Fail, the server encountered an unexpected error.                      |

## Get Log by Number of Lines
Returns an existing _**Automation Log**_ tail, by specific size.

> Configuration key to set logs folder is ```reportConfiguration.logsOut```.
> If not specified, the default logs directory is ```<current_directory>\Logs```.

```
GET /api/v3/logs/:log_id/size/:size
```

|Name            |Type  |Description                                                                                |
|----------------|------|-------------------------------------------------------------------------------------------|
|log_id          |string|The ID of the _**Automation Log**_. Will be the date in the following format ```yyyyMMdd```|
|size            |number|A fixed number of lines from the end of the log upwards.                                   |

#### Response Content
> The response is an array of log entries of media type ```text/plain```.
> Log entries are separated by an empty line.

The request body follows the same format as [Get Log](#get-log) response content.

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Automation Logs**_ is returned as part of the response.|
|404 |Not Found, the _**Automation Logs**_ were not found.                   |
|500 |Fail, the server encountered an unexpected error.                      |

## Export Log
Downloads an existing _**Automation Log**_ as _**zip**_ file.

> Configuration key to set logs folder is ```reportConfiguration.logsOut```.
> If not specified, the default logs directory is ```<current_directory>\Logs```.

```
GET /api/v3/logs/:log_id/download
```

|Name            |Type  |Description                                                                                |
|----------------|------|-------------------------------------------------------------------------------------------|
|log_id          |string|The ID of the _**Automation Log**_. Will be the date in the following format ```yyyyMMdd```|

#### Response Content
> The response is a file of media type ```application/zip```.

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Automation Logs**_ is returned as part of the response.|
|404 |Not Found, the _**Automation Logs**_ were not found.                   |
|500 |Fail, the server encountered an unexpected error.                      |