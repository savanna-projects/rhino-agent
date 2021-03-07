[Home](../Home.md 'Home')  

# API: Models
02/17/2021 - 55 minutes to read

## In This Article
* [Get Models](#get-models)
* [Get Model](#get-model)
* [Create Model](#create-model)
* [Create Model with Configuration](#create-model-with-configuration)
* [Get Associated Configurations](#get-associated-configurations)
* [Add Models to Collection](#add-models-to-collection)
* [Associate Model to Configuration](#associate-model-to-configuration)
* [Delete Model](#delete-model)
* [Delete Models](#delete-models)

_**Rhino Model**_ is a collection of entires which map a certain value into an expressive name (alias) for reuse of that specific value. For example, lets say that I have an element on my web page which is repeatable in all pages and I am using it in more than one test. In that case I can create a model which holds the locator information of that element, and call that model using the expressive name.  

For example
```
MY MODEL

name:    file menu
value:   //li[@id='file_menu']
type:    xpath
comment: An element that exists on all pages and is used by many tests.

Once the model is created I can refer it in my tests by calling the model entry name as such:

click on {file menu}
verify that {text} of {file menu} match {file}
...
```

> _**Information**_
>  
> 1. It is possible to use models to refer macros such as {{$date -format:yyyyMMddHHmmss}} or other textual combinations.
> 2. The comment filed is an optional filed and was deigned to add more information to the model.
> 3. It is possible to add context (a collection of key/value) to a model for further information about the model.
> 4. Models which are not associated to a collection or configuration will be applied to all tests (global models).

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

## Get Model
Returns an existing _**Rhino Model**_.

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

## Create Model with Configuration
Creates a new _**Rhino Model**_.

```
POST /api/v3/models/configuration_id
```

|Name            |Type  |Description                             |
|----------------|------|----------------------------------------|
|configuration_id|string|The ID of the _**Rhino Configuration**_.|

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
|404 |Not Found, the _**Configuration**_ was not found.                                                |
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

## Associate Model to Configuration
Add additional _**Rhino Models**_ into an existing configuration. If the model name is already exists on another model,
it will be ignored.

```
PATCH /api/v3/models/:model_id/configurations/:configuration_id
```

|Name            |Type  |Description                             |
|----------------|------|----------------------------------------|
|model_id        |string|The ID of the _**Rhino Model**_.        |
|configuration_id|string|The ID of the _**Rhino Configuration**_.|

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Model**_ was returned as part of the response.   |
|404 |Not Found, the _**Model**_ or _**Configuration**_ were not found.|
|500 |Fail, the server encountered an unexpected error.                |

## Delete Model
Deletes an existing _**Rhino Model**_.

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

## Delete Models
Deletes all existing _**Rhino Models**_.

```
DELETE /api/v3/models
```

> Please Note: Deleting a collection cannot be undone and it can affect test cases and configurations which were using the models.

### Response Codes
|Code|Description                                       |
|----|--------------------------------------------------|
|204 |Success, the _**Model**_ collections were deleted.|
|500 |Fail, the server encountered an unexpected error. |