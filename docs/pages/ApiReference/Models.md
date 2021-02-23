# API: Models
Use the following API methods to request details about _**Rhino Models**_ and to create or modify them.

## Get Models
Returns a list of available _**Rhino Models**_.

```
GET /api/v3/models
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
        "models": 1,
        "entries": 3
      },
      {
        "id": "ba6b3da7-1979-48ea-9b00-e30682f5f111",
        "configurations": [],
        "models": 1,
        "entries": 3
      }
    ]
  }
}
```

The example response includes 2 models, with 1 entries each and 2 configurations for one of them.

|Name          |Type  |Description                                              |
|--------------|------|---------------------------------------------------------|
|id            |string|The ID of the _**Rhino Models**_.                        |
|configurations|array |All _**Rhino Configurations**_ which are using the model.|
|models        |number|Total models under the models collection.                |
|entries       |number|Total entries (elements) under the model.                |

### Response Codes
|Code|Description                                                           |
|----|----------------------------------------------------------------------|
|200 |Success, the _**Rhino Models**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                     |

## Get Models Collection
Returns an existing _**Rhino Model**_ collection.

```
GET /api/v3/models/:collection_id
```

|Name         |Type  |Description                                |
|-------------|------|-------------------------------------------|
|collection_id|string|The ID of the _**Rhino Model**_ collection.|

### Response Content
Please see below for a typical response:

```js
[
  {
    "name": "Students Input Models",
    "entries": [
      {
        "name": "search students text-box",
        "value": "#SearchString",
        "type": "css selector",
        "comment": "Search students text-box on the top center panel under students page."
      },
      {
        "name": "search students button",
        "value": "//input[@id='SearchButton']",
        "type": "xpath",
        "comment": "Search students button on the top center panel under students page."
      }
    ],
    "context": {
      "pageUrl": "https://gravitymvctestapplication.azurewebsites.net/student"
    }
  },
  "name": "Students Table Models",
  ...
]
```

The following system fields are always included in the response:

#### General
|Name                   |Type  |Description                                                         |
|-----------------------|------|--------------------------------------------------------------------|
|name                   |string|The name of this _**Rhino Model**_.                                 |
|[entries](#model-entry)|array |A collection of _**Rhino Model Entry**_.                            |
|context                |object|A free style object which can be used to further describe the model.|

#### Model Entry
|Name   |Type  |Description                                                                                |
|-------|------|-------------------------------------------------------------------------------------------|
|name   |string|The name of the element (required and must be compliant with Rhino's language rules).      |
|value  |string|The value of selected locator (required).                                                  |
|type   |string|The type of selected locator. If no type specified, default is XPath.                      |
|comment|string|Any comment relevant for further describing this model entry (optional).                   |

### Response Codes
|Code|Description                                                            |
|----|-----------------------------------------------------------------------|
|200 |Success, the _**Models**_ were returned as part of the response.       |
|404 |Not Found, the _**Models**_ were not found under the models collection.|
|500 |Fail, the server encountered an unexpected error.                      |

## Create Model
Creates a new _**Rhino Model**_.

```
POST /api/v3/models
```

### Request Fields
The request body follows the same format as [Get Model](#get-model) response content.

### Request Example
```js
[
  {
    "name": "Students Input Models",
    "entries": [
      {
        "name": "search students text-box",
        "value": "#SearchString",
        "type": "css selector",
        "comment": "Search students text-box on the top center panel under students page."
      },
      {
        "name": "search students button",
        "value": "//input[@id='SearchButton']",
        "type": "xpath",
        "comment": "Search students button on the top center panel under students page."
      }
    ],
    "context": {
      "pageUrl": "https://gravitymvctestapplication.azurewebsites.net/student"
    }
  }
]
```

### Response Codes
|Code|Description                                                                                      |
|----|-------------------------------------------------------------------------------------------------|
|201 |Success, the _**Models Collection**_ created and identifier was returned as part of the response.|
|204 |No Content, the _**Models Collection**_ or a collection with the same name already exists.       |
|400 |Bad Request, the request is missing a mandatory field(s) or bad formatted.                       |
|500 |Fail, the server encountered an unexpected error.                                                |

## Get Associated Configurations
Returns a list of available _**Rhino Configurations**_ which are associated with this _**Rhino Model**_ collection.

```
GET /api/v3/models/:collection_id/configurations
```

|Name         |Type  |Description                                 |
|-------------|------|--------------------------------------------|
|collection_id|string|The ID of the _**Rhino Models**_ collection.|

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
|404 |Not Found, the _**Rhino Models**_ collection was not found.                   |
|500 |Fail, the server encountered an unexpected error.                             |

## Add Models to Collection
Add additional _**Rhino Models**_ into an existing collection. If the model name is already exists on another model,
it will be ignored.

```
PATCH /api/v3/models/:collection_id
```

|Name         |Type  |Description                                 |
|-------------|------|--------------------------------------------|
|collection_id|string|The ID of the _**Rhino Models**_ collection.|

### Request Fields
The request body follows the same format as [Get Model](#get-model) response content.

### Request Example
```js
[
  {
    "name": "Students Input Models",
    "entries": [
      {
        "name": "search students text-box",
        "value": "#SearchString",
        "type": "css selector",
        "comment": "Search students text-box on the top center panel under students page."
      },
      {
        "name": "search students button",
        "value": "//input[@id='SearchButton']",
        "type": "xpath",
        "comment": "Search students button on the top center panel under students page."
      }
    ],
    "context": {
      "pageUrl": "https://gravitymvctestapplication.azurewebsites.net/student"
    }
  }
]
```

### Response Codes
|Code|Description                                                        |
|----|-------------------------------------------------------------------|
|200 |Success, the _**Collection**_ was returned as part of the response.|
|404 |Not Found, the _**Collection**_ was not found.                     |
|500 |Fail, the server encountered an unexpected error.                  |

## Associate Configuration to Collection
Add additional _**Rhino Configuration**_ into an existing collection.

```
PATCH /api/v3/models/:collection_id/configurations/:configuration_id
```

|Name            |Type  |Description                                |
|----------------|------|-------------------------------------------|
|collection_id   |string|The ID of the _**Rhino Model**_ collection.|
|configuration_id|string|The ID of the _**Rhino Configuration**_.   |

### Response Codes
|Code|Description                                                           |
|----|----------------------------------------------------------------------|
|200 |Success, the _**Collection**_ was returned as part of the response.   |
|404 |Not Found, the _**Collection**_ or _**Configuration**_ were not found.|
|500 |Fail, the server encountered an unexpected error.                     |

## Delete Model Collection
Deletes an existing _**Rhino Model**_ collection.

```
DELETE /api/v3/models/:collection_id
```

|Name         |Type  |Description                                |
|-------------|------|-------------------------------------------|
|collection_id|string|The ID of the _**Rhino Model**_ collection.|

> Please Note: Deleting a collection cannot be undone and it can affect test cases and configurations which were using the models.

### Response Codes
|Code|Description                                      |
|----|-------------------------------------------------|
|204 |Success, the _**Model**_ collection was deleted. |
|404 |Not Found, the _**Model**_ was not found.        |
|500 |Fail, the server encountered an unexpected error.|

## Delete Model Collections
Deletes all existing _**Rhino Model**_ collections.

```
DELETE /api/v3/models
```

> Please Note: Deleting a collection cannot be undone and it can affect test cases and configurations which were using the models.

### Response Codes
|Code|Description                                       |
|----|--------------------------------------------------|
|204 |Success, the _**Model**_ collections were deleted.|
|500 |Fail, the server encountered an unexpected error. |