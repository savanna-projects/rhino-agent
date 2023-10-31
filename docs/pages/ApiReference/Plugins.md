[Home](../Home.md 'Home')  

# API: Plugins
02/16/2021 - 55 minutes to read

## In This Article
* [Get Plugin](#get-plugin)
* [Get Plugins](#get-plugins)
* [Create or Update Plugins](#create-or-update-plugins)
* [Delete Plugin](#delete-plugin)
* [Delete Plugins](#delete-plugins)

_**Rhino Plugin**_ is basically a reusable test case which can be called as an action in another test (e.g. works in the same way as shared steps). For example, a common scenario for creating a plugin is ```Login```. In most cases login is consist of 3 actions:

1. Type in user name.
2. Type in password.
3. click on the login button.  

These 3 actions will repeat themselves in every test, because every test needs a new login. It is possible to wrap these action into a plugin and call them as single action as such:

```
login {{$ --user_name:myUsername --password:my_password}}
```  

Using the plugin will remove the necessity to recreate 3 identical steps for each test and will remove the maintenance time if these steps needs to be updated - you will have to update only the plugin and it will affect all the tests that use it.

> _**Information**_
>  
> 1. It is possible to give the plugin any name you like, which later will be used to call the plugin inside a test.
> 2. It is possible to add as many parameters as you like to expose your plugin functionality.
> 3. It is possible to include assertions in the plugin.
> 4. It is possible to add more than one plugin in a single request.
> 5. Plugin syntax is almost identical to the syntax of a test case.
> 6. Once created, you will be able to access your plugins on all Rhino integrations including connectors and the recorder.

Use the following API methods to request details about _**Rhino Plugins**_ and to create or modify them.

## Get Plugin
Returns an existing _**Rhino Plugins**_ content.

```
GET /api/v3/plugins/:plugin_id
```

|Name     |Type  |Description                                                                                                |
|---------|------|-----------------------------------------------------------------------------------------------------------|
|plugin_id|string|The ID of the _**Rhino Plugin**_ this is the unique name of the plugin as given under "test-id" annotation.|

### Response Content
Please see below for a typical response:

```
[test-id] SearchStudent
[test-scenario] Search Student by First Name and Assert Page Address and Last Name

[test-parameters]
|Parameter |Description                                            |
|----------|-------------------------------------------------------|
|first_name|Student first name. Will be used for searching student.|
|last_name |Student last name. Will be used asserting results.     |

[test-actions]
1. send keys {first_name} into {#SearchString} using {css selector}
2. click on {#SearchButton} using {css selector}

[test-expected-results]
[2] verify that {url} match {student}
[2] verify that {attribute} of {#SearchString} using {css selector} from {value} match {first_name}
[2] verify that {text} of {//TD[contains(@id,'student_last_name_')]} match {last_name}

[test-examples]
|Example                                                       |Description                                                   |
|--------------------------------------------------------------|--------------------------------------------------------------|
|search student {{$ --first_name:Carson --last_name:Alexander}}|Performs student search by first name and validated last name.|
```

The following system fields are always included in the response:

> The response a Rhino Plugin Spec of media type `text/plain`.
> The fields are annotated following Rhino's language text format.

#### Mandatory Fields
|Name         |Type  |Description                                        |
|-------------|------|---------------------------------------------------|
|test-id      |text  |The **unique name** of the plugin.                 |
|test-scenario|text  |The title of the plugin.                           |
|test-actions |text  |Line separated list of the test actions to execute.|
|test-examples|object|At least one example of how to call your plugin.   |

The following system fields are sometimes included in the response:

#### Optional Fields
|Name                 |Type  |Description                                                                   |
|---------------------|------|------------------------------------------------------------------------------|
|test-expected-results|text  |Line separated list of the test expected results to execute.                  |
|test-parameters      |object|A list of parameters including a short description of what the parameter does.|

### Response Codes
|Code|Description                                                      |
|----|-----------------------------------------------------------------|
|200 |Success, the _**Rhino Plugin**_ returned as part of the response.|
|404 |Not Found, the provided _**Rhino Plugin**_ was not found.        |
|500 |Fail, the server encountered an unexpected error.                |

## Get Plugins
Returns a list of available _**Rhino Plugins**_ content.

```
GET /api/v3/plugins
```

### Response Content
The response body is an array of specs follows the same format as [Get Plugin](#get-plugin) response content.  
Please see below for a typical response:

```
[test-id] SearchStudent
[test-scenario] Search Student by First Name and Assert Page Address and Last Name

[test-parameters]
|Parameter |Description                                            |
|----------|-------------------------------------------------------|
|first_name|Student first name. Will be used for searching student.|
|last_name |Student last name. Will be used asserting results.     |

[test-actions]
1. send keys {first_name} into {#SearchString} using {css selector}
2. click on {#SearchButton} using {css selector}

[test-expected-results]
[2] verify that {url} match {student}
[2] verify that {attribute} of {#SearchString} using {css selector} from {value} match {first_name}
[2] verify that {text} of {//TD[contains(@id,'student_last_name_')]} match {last_name}

[test-examples]
|Example                                                       |Description                                                   |
|--------------------------------------------------------------|--------------------------------------------------------------|
|search student {{$ --first_name:Carson --last_name:Alexander}}|Performs student search by first name and validated last name.|

>>>

[test-id] SearchCourse
...
```

### Response Codes
|Code|Description                                                                   |
|----|------------------------------------------------------------------------------|
|200 |Success, the _**Rhino Plugin**_ collection returned as part of the response.  |
|404 |Not Found, no public plugins and no private plugins were found for the issuer.|
|500 |Fail, the server encountered an unexpected error.                             |

## Create or Update Plugins
Creates new or Updates existing one or more _**Rhino Plugin**_.

```
POST /api/v3/plugins?prvt=(false|true)
```

|Name|Type   |Description                                                                                         |
|----|-------|----------------------------------------------------------------------------------------------------|
|prvt|boolean|Set to true in order to create the plugin as a private plugin available only to the user created it.|

### Request Fields
The request body follows the same format as [Get Plugin](#get-plugin) response content.

### Request Example
```
[test-id] SearchStudent
[test-scenario] Search Student by First Name and Assert Page Address and Last Name

[test-parameters]
|Parameter |Description                                            |
|----------|-------------------------------------------------------|
|first_name|Student first name. Will be used for searching student.|
|last_name |Student last name. Will be used asserting results.     |

[test-actions]
1. send keys {first_name} into {#SearchString} using {css selector}
2. click on {#SearchButton} using {css selector}

[test-expected-results]
[2] verify that {url} match {student}
[2] verify that {attribute} of {#SearchString} using {css selector} from {value} match {first_name}
[2] verify that {text} of {//TD[contains(@id,'student_last_name_')]} match {last_name}

[test-examples]
|Example                                                       |Description                                                   |
|--------------------------------------------------------------|--------------------------------------------------------------|
|search student {{$ --first_name:Carson --last_name:Alexander}}|Performs student search by first name and validated last name.|

>>>

[test-id] SearchCourse
...
```

### Response Codes
|Code|Description                                                                                             |
|----|--------------------------------------------------------------------------------------------------------|
|200 |Success, some of the _**Plugin(s)**_ were not created and reasons were returned as part of the response.|
|201 |Created, the _**Plugin(s)**_ were created returned as part of the response.                             |
|500 |Fail, the server encountered an unexpected error.                                                       |

## Delete Plugin
Deletes an existing _**Rhino Plugin**_.

```
DELETE /api/v3/configurations/:plugin_id
```

|Name     |Type  |Description                                                                                                |
|---------|------|-----------------------------------------------------------------------------------------------------------|
|plugin_id|string|The ID of the _**Rhino Plugin**_ this is the unique name of the plugin as given under "test-id" annotation.|

> Please Note: Deleting a plugin cannot be undone and can affect test cases.

### Response Codes
|Code|Description                                                                         |
|----|------------------------------------------------------------------------------------|
|204 |Success, the _**Rhino Plugin**_ was deleted.                                        |
|404 |Not Found, the _**Rhino Plugin**_ was not found under the configurations collection.|
|500 |Fail, the server encountered an unexpected error.                                   |

## Delete Plugins
Deletes all existing _**Rhino Plugin**_.

```
DELETE /api/v3/configurations
```

> Please Note: Deleting a plugin cannot be undone and can affect test cases.

### Response Codes
|Code|Description                                      |
|----|-------------------------------------------------|
|204 |Success, the _**Rhino Plugins**_ were deleted.   |
|404 |Not Found, the _**Rhino Plugin**_ were not found.|
|500 |Fail, the server encountered an unexpected error.|