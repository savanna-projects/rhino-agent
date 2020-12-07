# API: Models
Use the following API methods to request details from _**Rhino Logs**_.

## Get Logs Files
Returns an existing _**Automation Logs**_ files list.

```
GET /api/v3/logs
```

#### Response Content

```js
[
  "RhinoApu-20201230.log",
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
Automation.Kdd.Agent Information: 0 : [SendKeys]; element [Username]; argument [userName] executed
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

## Get Log - Last Number of Lines
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

## Get Log - Download
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