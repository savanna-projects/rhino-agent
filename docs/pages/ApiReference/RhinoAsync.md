# API - Rhino
Use the following API methods to execute operations like executing configurations or tests collections against Rhino API.

## Invoke Configuration
Invokes (run) a _**Rhino Configuration**_.

```
POST /rhino/async/configurations/invoke
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
    "username": "rhino_user",
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
  "id": "beeac98b-9385-44e5-9edb-9c6275f94094",
  "statusCode": 201,
  "statusEndpoint": "/api/v3/rhino/async/status/beeac98b-9385-44e5-9edb-9c6275f94094"
}
```

The following system fields are always included in the response:

#### General
|Name          |Type  |Description                                                                                              |
|--------------|------|---------------------------------------------------------------------------------------------------------|
|id            |string|The unique identifier of this _**Rhino Test Run**_.                                                      |
|statusCode    |number|The status code of the async _**Rhino Test Run**_ state creation.                                        |
|statusEndpoint|string|The endpoint reference from which you it is possible to get the status of the async _**Rhino Test Run**_.|

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
















































