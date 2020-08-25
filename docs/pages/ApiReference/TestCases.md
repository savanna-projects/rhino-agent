# API: Tests
Use the following API methods to request details about _**Rhino Test Cases**_ and to create or modify them.

## Get Test Case Collections
Returns a list of available _**Rhino Test Cases**_ collections.

```
GET /api/v3/tests
```

#### Response Content
```js
{
  "data": {
    "collection": [
      {
        "id": "1ed4ea1c-9959-40d7-b40e-717b8fc1cfb4",
        "configurations": [
          "03d1cd94-5e38-43d8-b010-e932d92f9067",
          "8bed8025-3cgf-52g1-0919-533cbc6d523c"
        ],
        "tests": 3
      },
      {
        "id": "ba6b3da7-1979-48ea-9b00-e30682f5f111",
        "configurations": [],
        "tests": 3
      }
    ]
  }
}
```

The example response includes 2 collections, with 3 tests in each and 2 configurations for one of them.

|Name          |Type  |Description                                                   |
|--------------|------|--------------------------------------------------------------|
|id            |string|The ID of the _**Rhino Tests Collection**_.                   |
|configurations|array |All _**Rhino Configurations**_ which are using the collection.|
|tests         |number|Total models under the models collection.                     |

### Response Codes
|Code|Description                                                                            |
|----|---------------------------------------------------------------------------------------|
|200 |Success, the _**Rhino Tests Collection**_ information returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                                      |

## Get Test Case Collection
Returns an existing _**Rhino Test Case**_ collection (test suite content).

```
GET /api/v3/tests/:collection_id
```

|Name         |Type  |Description                                    |
|-------------|------|-----------------------------------------------|
|collection_id|string|The ID of the _**Rhino Test Case**_ collection.|

### Response Content
Please see below for a typical response:

```
[test-id] rhino-documentation-#001
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
    "address": "https://gravitymvctestapplication.azurewebsites.net/student",
    "address-expected": "gravitymvctestapplication.azurewebsites.net",
    "search-text-box": "//input[@id='SearchString']",
    "search-button": "#SearchButton",
    "students-table": "//td[@id]"
  }
]

>>>

[test-id] rhino-documentation-#002
...
```

The following system fields are always included in the response:

> The response is an array of Rhino Test Spec of media type `text/plain`.
> Scenarios are separated by an empty line, followed by `>>>` followed by another empty line.
> The fields are annotated following Rhino's language text format.

#### Mandatory Fields
|Name                 |Type|Description                                                  |
|---------------------|----|-------------------------------------------------------------|
|test-id              |text|The unique ID of the test case.                              |
|test-scenario        |text|The title of the test case.                                  |
|test-actions         |text|Line separated list of the test actions to execute.          |

The following system fields are sometimes included in the response:

#### Optional Fields
|Name                 |Type|Description                                                            |
|---------------------|----|-----------------------------------------------------------------------|
|test-expected-results|text|Line separated list of the test expected results to execute.           |
|test-data-provider   |text|JSON or Markdown format table (string, string) for data driven testing.|
|test-priority        |text|The test priority level - must include number i.e. "1 - High".         |
|test-severity        |text|The test severity level - must include number i.e. "4 - Low".          |
|test-tolerance       |text|The test tolerance level - must include decimal number i.e. "80.5%".   |

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Rhino Tests Cases**_ returned as part of the response. |
|404 |Not Found, the provided _**Rhino Test Case Collection**_ was not found.|
|500 |Fail, the server encountered an unexpected error.                      |

## Create Test Case Collection
Creates a new _**Rhino Test Case Collection**_.

```
POST /api/tests/:configuration_id
```

|Name            |Type  |Description                                                                             |
|----------------|------|----------------------------------------------------------------------------------------|
|configuration_id|string|The ID of the configuration group under which to create the tests collection (optional).|

### Request Fields
New _**Rhino Test Case Collection**_ using the same response format as [Get Test Case Collection](#get-test-case-collection).  

> Please note, it is possible to create an empty collection without provided any information in the request body.

#### Request Sample
```
[test-id] rhino-documentation-#001
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

[test-id] rhino-documentation-#002
...
```

### Response Fields
|Name |Type  |Description              |
|-----|------|-------------------------|
|id   |string|The ID of the collection.|

#### Response Sample
```js
{
  "data": {
    "id": "73e21d1b-770b-4347-805a-eae4f622a146"
  }
}
```

### Response Codes
|Code|Description                                                    |
|----|---------------------------------------------------------------|
|201 |Success, the _**Rhino Test Case Collection**_ was created.     |
|404 |Not Found, the provide _**Rhino Configuration**_ was not found.|
|500 |Fail, the server encountered an unexpected error.              |

## Get Associated Configurations
Returns a list of available _**Rhino Configurations**_ which are associated with this _**Rhino Test Case**_ collection.

```
GET /api/v3/tests/:collection_id/configurations
```

|Name         |Type  |Description                                    |
|-------------|------|-----------------------------------------------|
|collection_id|string|The ID of the _**Rhino Test Case**_ collection.|

#### Response Content
```js
{
  "data": {
    "configurations": [
      "03d1cd94-5e38-43d8-b010-e932d92f9067",
      "8bed8025-3cgf-52g1-0919-533cbc6d523c"
    ]
  }
}
```

The example response includes 2 configuration which are associated with this collection.

|Name          |Type  |Description                                      |
|--------------|------|-------------------------------------------------|
|configurations|array |All _**Rhino Models**_ which are using the model.|

### Response Codes
|Code|Description                                                                   |
|----|------------------------------------------------------------------------------|
|200 |Success, the _**Rhino Configurations**_ were returned as part of the response.|
|404 |Not Found, the _**Rhino Test Case**_ collection was not found.                |
|500 |Fail, the server encountered an unexpected error.                             |

## Add Test Cases to Collection
Add additional _**Rhino Test Cases**_ into an existing collection.  

> Please note, there is no duplication check here, and you can add tests with the same ID.
> However, Rhino Engine, will not execute them, since it will distinct all IDs before running automation.

```
PATCH /api/v3/tests/:collection_id
```

|Name         |Type  |Description                                    |
|-------------|------|-----------------------------------------------|
|collection_id|string|The ID of the _**Rhino Test Case**_ collection.|

### Request Fields
The request body follows the same format as [Get Test Case Collection](#get-test-case-collection) response content.

### Request Example
```
[test-id] rhino-documentation-#003
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
    "address": "https://gravitymvctestapplication.azurewebsites.net/student",
    "address-expected": "gravitymvctestapplication.azurewebsites.net",
    "search-text-box": "//input[@id='SearchString']",
    "search-button": "#SearchButton",
    "students-table": "//td[@id]"
  }
]

>>>

[test-id] rhino-documentation-#004
...
```

### Response Codes
|Code|Description                                                        |
|----|-------------------------------------------------------------------|
|200 |Success, the _**Collection**_ was returned as part of the response.|
|400 |Bad Request, no test cases were provided in the request body.      |
|404 |Not Found, the _**Collection**_ was not found.                     |
|500 |Fail, the server encountered an unexpected error.                  |

## Associate Configuration to Collection
Add additional _**Rhino Configuration**_ into an existing collection.

```
PATCH /api/v3/tests/:collection_id/configurations/:configuration_id
```

|Name            |Type  |Description                                    |
|----------------|------|-----------------------------------------------|
|collection_id   |string|The ID of the _**Rhino Test Case**_ collection.|
|configuration_id|string|The ID of the _**Rhino Configuration**_.       |

### Response Codes
|Code|Description                                                           |
|----|----------------------------------------------------------------------|
|204 |Success, the _**Configuration**_ was applied to the _**Collection**_. |
|404 |Not Found, the _**Collection**_ or _**Configuration**_ were not found.|
|500 |Fail, the server encountered an unexpected error.                     |

## Delete Test Case Collection
Deletes an existing _**Rhino Test Case**_ collection.

```
DELETE /api/v3/tests/:collection_id
```

|Name         |Type  |Description                                    |
|-------------|------|-----------------------------------------------|
|collection_id|string|The ID of the _**Rhino Test Case**_ collection.|
  
> Please Note: Deleting a collection cannot be undone and it can affect the configurations which were using the test cases.
  
### Response Codes
|Code|Description                                             |
|----|--------------------------------------------------------|
|204 |Success, the _**Test Case**_ collection was deleted.    |
|404 |Not Found, the _**Test Case**_ collection was not found.|
|500 |Fail, the server encountered an unexpected error.       |

## Delete Test Case Collections
Deletes all existing _**Rhino Test Case**_ collections.

```
DELETE /api/v3/tests
```
  
> Please Note: Deleting a collection cannot be undone and it can affect the configurations which were using the test cases.
  
### Response Codes
|Code|Description                                           |
|----|------------------------------------------------------|
|204 |Success, the _**Test Case**_ collections were deleted.|
|500 |Fail, the server encountered an unexpected error.     |