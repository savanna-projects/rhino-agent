# Deployment
Rhino API runs .NET Core 3.1.

## Supported OS
Please read [here](https://github.com/dotnet/core/blob/master/release-notes/3.1/3.1-supported-os.md) for OS support matrix.

## Requierments
1. Rhino API deployed on premise. Please [read here](./Deploymnet.md) for more information how to download and deploy Rhino API.

## Set Server Site Settings
1. Under the folder you have deployed Rhino API, open the file ```appSettings.json``` using any text editor.

### Report Configuration
This settings section is responsible for configuring the folders and locations in which Rhino will create reports and logs.  

|Name            |Type   |Description                                                                                                  |
|----------------|-------|-------------------------------------------------------------------------------------------------------------|
|reportOut       |string |The directory in which to save reports (e.g. ```D:\\sites\\RhinoOutputs\\Reports\\Rhino```).                 |
|logsOut         |string |The directory in which to save logs (e.g. ```D:\\sites\\RhinoOutputs\\Logs```).                              |
|reporters       |array  |Reporters implementations to use with this configuration (e.g. ```["reporterBasic", "reporterWarehouse"]```).|
|connectionString|string |The reporter connection string (if needed or used by the provided implementations).                          |
|dataProvider    |string |The reporter data provider (if needed or used by provided implementations).                                  |
|archive         |boolean|When set to true, will archive the report out folder as zip file and delete the original folder.             |

### Screenshots Configuration
This settings section is responsible for configuring the folders and locations in which Rhino will create screenshots archives.  

|Name             |Type   |Description                                                                                               |
|-----------------|-------|----------------------------------------------------------------------------------------------------------|
|keepOriginal     |boolean|When set to true, will keep the original file created by Gravity engine, when creating a new Rhino report.|
|screenshotsOut   |decimal|The directory in which to save automatic screenshots (e.g. ```D:\\sites\\RhinoOutputs\\Screenshots```).   |

[Next Step: Run Verification Tests](./VerifyDeploymnet.md)