[Home](../Home.md 'Home')  

# Server Settings
05/12/2020 - 10 minutes to read

## In This Article
* [Prerequisites](#prerequisites)
* [Set Server Site Settings](#set-server-site-settings)

## Prerequisites
1. Rhino API deployed on premise. Please refer to the different deployment options for more information how to download and deploy Rhino API.

## Set Server Site Settings
> The default storage folder in the docker version is ```outputs```. Please read for more information how to edit files on docker.
> * [Use Volumes](https://docs.docker.com/storage/volumes/)
> * [Docker exec](https://docs.docker.com/engine/reference/commandline/exec/)  

Under the folder you have deployed Rhino API, open the file ```appSettings.json``` using any text editor.

### Report Configuration
> A list of all available reporters on your server can be fetch by sending the following GET request:
> _**https://{server_address}:9001/api/v3/widget/reporters**_.  

This settings section is responsible for configuring the folders and locations in which Rhino will create reports and logs.  

|Name            |Type   |Description                                                                                                    |
|----------------|-------|---------------------------------------------------------------------------------------------------------------|
|reportOut       |string |The directory in which to save reports (e.g. ```D:\\sites\\RhinoOutputs\\Reports\\Rhino```).                   |
|logsOut         |string |The directory in which to save logs (e.g. ```D:\\sites\\RhinoOutputs\\Logs```).                                |
|reporters       |array  |Reporters implementations to use with this configuration (e.g. ```["reporter_basic", "reporter_warehouse"]```).|
|archive         |boolean|When set to true, will archive the report out folder as zip file and delete the original folder.               |

### Screenshots Configuration
This settings section is responsible for configuring the folders and locations in which Rhino will create screenshots archives.  

|Name             |Type   |Description                                                                                               |
|-----------------|-------|----------------------------------------------------------------------------------------------------------|
|keepOriginal     |boolean|When set to true, will keep the original file created by Gravity engine, when creating a new Rhino report.|
|screenshotsOut   |decimal|The directory in which to save automatic screenshots (e.g. ```D:\\sites\\RhinoOutputs\\Screenshots```).   |

[Next Step: Run Verification Tests](./VerifyDeploymnet.md)

## See Also
* [Rhino Agent Deployment - Process Host](./Deployment.md 'Deployment')
* [Rhino Agent Deployment - Docker](./DeploymentDocker.md 'DeploymentDocker')
* [Rhino Agent Deployment - On IIS (Internet Information Service)](./DeploymentIIS.md 'DeploymentIIS')