[Home](../Home.md 'Home')  

# API: Meta (static data)
02/16/2021 - 55 minutes to read

## In This Article
* [Get Plugins](#get-plugins)
* [Get Plugin](#get-plugin)
* [Get Assertions](#get-assertions)
* [Get Assertion](#get-assertion)
* [Get Connectors](#get-connectors)
* [Get Connector](#get-connector)
* [Get Drivers](#get-drivers)
* [Get Driver](#get-driver)
* [Get Locators](#get-locators)
* [Get Locator](#get-locator)
* [Get Macros](#get-macros)
* [Get Macro](#get-macro)
* [Get Operators](#get-operators)
* [Get Operator](#get-operator)
* [Get Reporters](#get-reporters)
* [Get Reporter](#get-reporter)
* [Get Version](#get-version)

The _**Meta**_ service can provide information about the resource and plugins available for Rhino. This information can vary from server to server.

> _**Information**_
>
> The meta data is generated automatically based on the different implementations in your server, for example, if you have implemented an action, a plugin or a reporter, the meta service will return them for you.  

Use the following API methods to request for available _**Static Data**_.

## Get Plugins
Returns a list of available _**Plugins**_ (both _**Rhino**_ and _**Code**_).

```
GET /api/v3/meta/plugins
```

### Response Content
The response body is an array of object following the same format as [Get Plugin](#get-plugin) response content.

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Plugins**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                |

## Get Plugin
Returns a single available _**Plugin**_ (both _**Rhino**_ and _**Code**_).

```
GET /api/v3/meta/plugins/:key
```

|Name|Type  |Description                                                                   |
|----|------|------------------------------------------------------------------------------|
|key |string|The key of the _**Plugins**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "source": "code",
  "key": "Click",
  "literal": "click",
  "verb": "on",
  "entity": {
    "Name": "Click",
    "TestOn": "https://gravitymvctestapplication.azurewebsites.net/instructor",
    "Examples": [
      {
        "description": "Clicks the mouse on the specified element.",
        "actionExample": {
          "actionType": "click",
          "locator": "",
          "reference": 0,
          "repeatReference": 0,
          "actions": [],
          "elementAttributeToActOn": "",
          "elementToActOn": "(//table//a)[1]",
          "regularExpression": ".*",
          "argument": ""
        },
        "literalExample": "click on {(//table//a)[1]}",
        "comment": "Executed on [Edit] page. Select any instructor and click on [Edit] link."
      },
      {
        "description": "Clicks the mouse at the last known mouse coordinates.",
        "actionExample": {
          "actionType": "Click",
          "locator": "",
          "reference": 0,
          "repeatReference": 0,
          "actions": [],
          "elementAttributeToActOn": "",
          "elementToActOn": "",
          "regularExpression": ".*",
          "argument": ""
        },
        "literalExample": "click"
      },
      {
        "description": "Clicks the mouse on the specified element. If alert is present after that click, it will be dismissed and the click action repeats. The action repeats until no alert or until page load timeout reached.",
        "actionExample": {
          "actionType": "Click",
          "locator": "",
          "reference": 0,
          "repeatReference": 0,
          "actions": [],
          "elementAttributeToActOn": "",
          "elementToActOn": "(//table//a)[1]",
          "regularExpression": ".*",
          "argument": "{{$ --until:noalert}}"
        },
        "literalExample": "click {{$ --until:noalert}} on {(//table//a)[1]}"
      }
    ],
    "Summary": "Clicks the mouse at the last known mouse coordinates or on the specified element.",
    "Description": "Clicks the mouse at the last known mouse coordinates or on the specified element. If the click causes a new page to load, the OpenQA.Selenium.IWebElement.Click method will attempt to block until the page has loaded.",
    "Scope": [
      "web",
      "mobile-web",
      "mobile-native"
    ],
    "Properties": {
      "argument": "Click action conditions and additional information.",
      "elementToActOn": "The locator value by which the element will be found.",
      "locator": "The locator type by which the element will be found."
    },
    "CliArguments": {
      "until": "Repeats the click action until condition is met. Available conditions are: ['NoAlert']."
    },
    "Protocol": {
      "endpoint": "none",
      "w3c": "https://www.w3.org/TR/webdriver/#actions"
    }
  }
}
```

The following system fields are always included in the response:  

|Name   |Type  |Description                                                                                           |
|-------|------|------------------------------------------------------------------------------------------------------|
|source |string|The plugin source, can be either _**Rhino**_ or _**Code**_.                                           |
|key    |string|The unique identifier of the plugin. Must be PascalCase.                                              |
|literal|string|The literal representation of the plugin as used by Rhino's language.                                 |
|verb   |string|The verb used to identify the plugin target as used by Rhino's language.                              |
|entity |object|The underline ActionAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                    |
|----|---------------------------------------------------------------|
|200 |Success, the _**Plugin**_ was returned as part of the response.|
|404 |Not Found, the _**Plugin**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.              |

## Get Assertions
Returns a list of available _**Assertions**.

```
GET /api/v3/meta/assertions
```

### Response Content
The response body is an array of object following the same format as [Get Assertion](#get-assertion) response content.

### Response Codes
|Code|Description                                                         |
|----|--------------------------------------------------------------------|
|200 |Success, the _**Assertions**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                   |

## Get Assertion
Returns a single available _**Assertion**_.

```
GET /api/v3/meta/assertions/:key
```

|Name|Type  |Description                                                                     |
|----|------|--------------------------------------------------------------------------------|
|key |string|The key of the _**Assertion**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "attribute",
  "literal": "attribute",
  "verb": "on",
  "entity": {
    "Name": "attribute"
  }
}
```

The following system fields are always included in the response:  

|Name   |Type  |Description                                                                                                 |
|-------|------|------------------------------------------------------------------------------------------------------------|
|key    |string|The unique identifier of the assertion.                                                                     |
|literal|string|The literal representation of the assertion as used by Rhino's language.                                    |
|verb   |string|The verb used to identify the assertion target as used by Rhino's language.                                 |
|entity |object|The underline AssertMethodAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                       |
|----|------------------------------------------------------------------|
|200 |Success, the _**Assertion**_ was returned as part of the response.|
|404 |Not Found, the _**Assertion**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.                 |

## Get Connectors
Returns a list of available _**Connectors**.

```
GET /api/v3/meta/connectors
```

### Response Content
The response body is an array of object following the same format as [Get Connector](#get-connector) response content.

### Response Codes
|Code|Description                                                         |
|----|--------------------------------------------------------------------|
|200 |Success, the _**Connectors**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                   |

## Get Connector
Returns a single available _**Connector**_.

```
GET /api/v3/meta/connectors/:key
```

|Name|Type  |Description                                                                     |
|----|------|--------------------------------------------------------------------------------|
|key |string|The key of the _**Connector**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "connector_azure",
  "literal": "connector azure",
  "verb": "",
  "entity": {
    "Name": "Connector - Azure DevOps & Team Foundation Server (TFS)",
    "Description": "Allows to execute Rhino Specs from Azure DevOps or Team Foundation Server Test Case work items and report back as Test Runs.",
    "Value": "connector_azure"
  }
}
```

The following system fields are always included in the response:  

|Name   |Type  |Description                                                                                              |
|-------|------|---------------------------------------------------------------------------------------------------------|
|key    |string|The unique identifier of the connector.                                                                  |
|literal|string|The literal representation of the connector as used by Rhino's language.                                 |
|verb   |string|The verb used to identify the connector target as used by Rhino's language.                              |
|entity |object|The underline ConnectorAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                       |
|----|------------------------------------------------------------------|
|200 |Success, the _**Connector**_ was returned as part of the response.|
|404 |Not Found, the _**Connector**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.                 |

## Get Drivers
Returns a list of available _**Drivers**.

```
GET /api/v3/meta/drivers
```

### Response Content
The response body is an array of object following the same format as [Get Driver](#get-driver) response content.

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Drivers**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                |

## Get Driver
Returns a single available _**Driver**_.

```
GET /api/v3/meta/drivers/:key
```

|Name|Type  |Description                                                                  |
|----|------|-----------------------------------------------------------------------------|
|key |string|The key of the _**Driver**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "AndroidDriver",
  "literal": "android driver",
  "verb": "",
  "entity": 0
}
```

The following system fields are always included in the response:  

|Name   |Type  |Description                                                                                                 |
|-------|------|------------------------------------------------------------------------------------------------------------|
|key    |string|The unique identifier of the driver.                                                                        |
|literal|string|The literal representation of the driver as used by Rhino's language.                                       |
|verb   |string|The verb used to identify the driver target as used by Rhino's language.                                    |
|entity |object|The underline DriverMethodAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                    |
|----|---------------------------------------------------------------|
|200 |Success, the _**Driver**_ was returned as part of the response.|
|404 |Not Found, the _**Driver**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.              |

## Get Locators
Returns a list of available _**Locators**.

```
GET /api/v3/meta/locators
```

### Response Content
The response body is an array of object following the same format as [Get Locator](#get-locator) response content.

### Response Codes
|Code|Description                                                       |
|----|------------------------------------------------------------------|
|200 |Success, the _**Locators**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                 |

## Get Locator
Returns a single available _**Locator**_.

```
GET /api/v3/meta/locators/:key
```

|Name|Type  |Description                                                                   |
|----|------|------------------------------------------------------------------------------|
|key |string|The key of the _**Locator**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "Id",
  "literal": "id",
  "verb": "using",
  "entity": {
    "examples": [
      {
        "description": "Gets a mechanism to find elements by their ID.",
        "example": "click on {element_id} using {id}"
      }
    ]
  }
}
```

The following system fields are always included in the response:  

|Name    |Type  |Description                                                                                                       |
|--------|------|------------------------------------------------------------------------------------------------------------------|
|key     |string|The unique identifier of the locator.                                                                             |
|literal |string|The literal representation of the locator as used by Rhino's language.                                            |
|verb    |string|The verb used to identify the locator target as used by Rhino's language.                                         |
|entity  |object|The underline LocatorAttribute (By class) as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                     |
|----|----------------------------------------------------------------|
|200 |Success, the _**Locator**_ was returned as part of the response.|
|404 |Not Found, the _**Locator**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.               |

## Get Macros
Returns a list of available _**Macros**.

```
GET /api/v3/meta/macros
```

### Response Content
The response body is an array of object following the same format as [Get Macro](#get-macro) response content.

### Response Codes
|Code|Description                                                     |
|----|----------------------------------------------------------------|
|200 |Success, the _**Macros**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.               |

## Get Macro
Returns a single available _**Macro**_.

```
GET /api/v3/meta/macros/:key
```

|Name|Type  |Description                                                                 |
|----|------|----------------------------------------------------------------------------|
|key |string|The key of the _**Macro**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "alertxt",
  "literal": "alertxt",
  "verb": "",
  "entity": {
    "name": "alertxt",
    "testOn": "https://gravitymvctestapplication.azurewebsites.net/instructor",
    "examples": [
      {
        "description": "Gets the [innerText] of the alert, without any leading or trailing whitespace and with other whitespace collapsed.",
        "macroExample": "{{$alertxt}}"
      },
      {
        "description": "Gets the first [innerText] match of the alert, without any leading or trailing whitespace and with other whitespace collapsed.",
        "macroExample": "{{$alertxt --regex:mock}}"
      }
    ],
    "summary": "Gets the text from alert dialog.",
    "description": "Gets the text from alert dialog. Can be conjured with regular expression under the pattern switch.",
    "scope": [
      "web"
    ],
    "properties": null,
    "cliArguments": {
      "regex": "(Optional) A pattern by which the information will be extracted, will return the first match."
    },
    "protocol": {
      "endpoint": "/session/{session-id}/alert/text",
      "w3c": "https://www.w3.org/TR/webdriver1/#get-alert-text"
    }
  }
}
```

The following system fields are always included in the response:  

|Name    |Type  |Description                                                                                          |
|--------|------|-----------------------------------------------------------------------------------------------------|
|key     |string|The unique identifier of the macro.                                                                  |
|literal |string|The literal representation of the macro as used by Rhino's language.                                 |
|verb    |string|The verb used to identify the macro target as used by Rhino's language.                              |
|entity  |object|The underline MacroAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                   |
|----|--------------------------------------------------------------|
|200 |Success, the _**Macro**_ was returned as part of the response.|
|404 |Not Found, the _**Macro**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.             |

## Get Operators
Returns a list of available _**Operators**.

```
GET /api/v3/meta/operators
```

### Response Content
The response body is an array of object following the same format as [Get Operator](#get-operator) response content.

### Response Codes
|Code|Description                                                        |
|----|-------------------------------------------------------------------|
|200 |Success, the _**Operators**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                  |

## Get Operator
Returns a single available _**Operator**_.

```
GET /api/v3/meta/operators/:key
```

|Name|Type  |Description                                                                    |
|----|------|-------------------------------------------------------------------------------|
|key |string|The key of the _**Operator**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "EQ",
  "literal": "equal",
  "verb": "",
  "entity": {
    "examples": [
      {
        "description": "Compares two values to be equal or not.",
        "example": "verify that {title} equal {my page title}"
      }
    ]
  }
}
```

The following system fields are always included in the response:  

|Name    |Type  |Description                                                                                             |
|--------|------|--------------------------------------------------------------------------------------------------------|
|key     |string|The unique identifier of the operator.                                                                  |
|literal |string|The literal representation of the operator as used by Rhino's language.                                 |
|verb    |string|The verb used to identify the operator target as used by Rhino's language.                              |
|entity  |object|The underline OperatorAttribute as loaded into the domain by Rhino Engine and as implemented in Gravity.|

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Operator**_ was returned as part of the response.|
|404 |Not Found, the _**Operator**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.                |

## Get Reporters
Returns a list of available _**Reporters**.

```
GET /api/v3/meta/reporters
```

### Response Content
The response body is an array of object following the same format as [Get Reporter](#get-reporter) response content.

### Response Codes
|Code|Description                                                        |
|----|-------------------------------------------------------------------|
|200 |Success, the _**Reporters**_ were returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.                  |

## Get Reporter
Returns a single available _**Reporter**_.

```
GET /api/v3/meta/reporters/:key
```

|Name|Type  |Description                                                                    |
|----|------|-------------------------------------------------------------------------------|
|key |string|The key of the _**Reporter**_ to find by. **Note**: the name is case sensitive.|

### Response Content
Please see below for a typical response:

```js
{
  "key": "reporter_basic",
  "literal": "reporter basic",
  "verb": "",
  "entity": {
    "Description": "The default Rhino HTML Reporter. A rich HTML Report with all test results and quality matrix.",
    "Name": "reporter_basic"
  }
}
```

The following system fields are always included in the response:  

|Name    |Type  |Description                                                                                           |
|--------|------|------------------------------------------------------------------------------------------------------|
|key     |string|The unique identifier of the reporter.                                                                |
|literal |string|The literal representation of the reporter as used by Rhino's language.                               |
|verb    |string|The verb used to identify the reporter target as used by Rhino's language.                            |
|entity  |object|The underline ReporterAttribute as loaded into the domain by Rhino Engine and as implemented in Rhino.|

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Reporter**_ was returned as part of the response.|
|404 |Not Found, the _**Reporter**_ was not found by the provided key. |
|500 |Fail, the server encountered an unexpected error.                |

## Get Version
Returns the version the Rhino Server instance (if available).

```
GET /api/v3/meta/version
```

### Response Content
Please see below for a typical response:

```
2021.01.30.1
```

### Response Codes
|Code|Description                                                     |
|----|----------------------------------------------------------------|
|200 |Success, the _**Version**_ was returned as part of the response.|
|500 |Fail, the server encountered an unexpected error.               |