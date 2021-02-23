[Home](../Home.md 'Home')  

# API: Utilities
02/16/2021 - 10 minutes to read

## In This Article
* [Get Usage](#get-usage)

The _**Utilities**_ service can provide different information about the user account, such as minutes used, available features, the next minutes reset date and more.  

> _**Information**_
>
> You must provide the user name and password in the request header using _**Basic Authentication**_.
> For more information about how to create an account, please refer to [Register Documentation](../GettingsStarted/Register.md).  

Use the following API methods to request details about your account status.

## Get Usage

Returns an information about the automation usage of the requested account.

```
GET https://gravityapi.azurewebsites.net/api/account/usage
```

### Authrization
```
BASIC User:Password
```

### Response Content
```js
{
  "id": "f2893a73-a842-4a23-83d9-f5a81f00a4cf",
  "email": "automation@rhino.api",
  "package": "internal",
  "features": [
    "all"
  ],
  "freeMinutes": 600,
  "packageMinutes": -1,
  "totalMinutes": -1,
  "pricePerMinute": 0,
  "minutesUsed": 0,
  "minutesLeft": -1,
  "resetOn": "03-01-2021",
  "support": [
    "gravity.customer-services@outlook.com",
    "gravity.api@outlook.com",
    "rhino.api@gmail.com",
    "https://github.com/savanna-projects/rhino-agent"
  ]
}
```

The following system fields are always included in the response:

#### General
|Name          |Type  |Description                                                                                  |
|--------------|------|---------------------------------------------------------------------------------------------|
|id            |string|The account unique identifier.                                                               |
|email         |string|The email used for creating the account.                                                     |
|package       |string|The package assigned to the account.                                                         |
|features      |array |A collection of all available features for the account.                                      |
|freeMinutes   |number|The amount of free minutes for the account.                                                  |
|packageMinutes|number|The amount of paid minutes for the account.                                                  |
|pricePerMinute|number|The unit price of paid minute.                                                               |
|minutesUsed   |number|The amount of minutes already used in the account for the current month (will reset monthly).|
|minutesLeft   |number|The amount of minutes left for use in the account for the current month (will reset monthly).|
|resetOn       |date  |The date of the next minutes reset.                                                          |
|support       |array |A list of emails and other support channels.                                                 |

### Response Codes
|Code|Description                                                                   |
|----|------------------------------------------------------------------------------|
|200 |Success, the _**Package Usage**_ information returned as part of the response.|
|401 |Unauthorized, the credential provided are missing or not valid.               |